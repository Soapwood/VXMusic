using System.Xml;
using UnityEditorInternal;

namespace AiUnity.CLog.Editor
{
    using AiUnity.CLog.Core;
    using AiUnity.CLog.Core.Common;
    using AiUnity.CLog.Core.Loggers;
    using AiUnity.Common.Attributes;
    using AiUnity.Common.Editor.Styles;
    using AiUnity.Common.Extensions;
    using AiUnity.Common.InternalLog;
    using AiUnity.Common.Log;
    using AiUnity.Common.Types;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using UnityEditor;
    using UnityEditor.AnimatedValues;
    using UnityEngine;

    /// <summary>
    /// CLog editor window to create, edit, test clog XML configuration.
    /// </summary>
    /// <seealso cref="UnityEditor.EditorWindow" />
    [Serializable]
    public class CLogEditor : EditorWindow
    {
        #region const fields
        // Default configuration file created upon user request
        // The CLog standard namespace is xmlns=""http://www.clog-project.org/schemas/CLog.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
        private const string DefaultConfig = @"<?xml version=""1.0"" encoding=""utf-8""?>
  <clog buildLevels="""">
	<targets>
		<target name=""UnityConsole"" type=""UnityConsole""/>
	</targets>
  </clog>";

        // Tooltip for config file selection GUI
        private const string ConfigTooltip = "Fixed location of CLog XML configuration file.  The GUI controls below provide the means to modify the XML indirectly (recommended) and directly (XML Viewer).  The CLog framework is solely configured by reading the configuration XML resource file at runtime, leaving it completely independent from the Unity Editor.";
        // Tooltip for Source/DLL selection GUI
        private const string SourceTooltip = @"Specifies if CLog DLLs (recommended) or Source Code is used during compilation.  DLLs compile faster and enable double-click of log messages to bring up corresponding IDE editor line.  Source code allows you to investigate, modify, and extend the inner workings of CLog.";
        // Tooltip for platform selection GUI
        private const string PlatformsTooltip = @"Specifies which Unity platforms builds include CLog logging.  Logging statements on unselected platforms will be compiled out of existence.  To select additional platforms at least one Build level must be selected.  Note the standalone editor platform is required.";
        // Tooltip for global level selection GUI
        private const string BuildLevelsTooltip = @"Specifies which logging levels are included in Unity builds.  Logging statements using unselected levels will be compiled out of existence.";
        // Tooltip for Internal Level selection GUI
        private const string InternalLevelsTooltip = @"Specifies which logging levels are enabled for CLog internal messages (Debug feature).";
        // Tooltip for CLog internal debug GUI
        private const string TooltipCLogDebugTooltip = @"Provides extra options and features to help debug CLog.  Useful in diagnosing configuration issues or for developing extensions.";
        #endregion

        #region private fields
        private XNamespace DefaultNamespace;
        private Dictionary<XElement, AnimBool> foldoutStates;
        private Dictionary<XElement, AnimBool> showAdvancedTarget;
        private bool IsConfigLoaded = false;
        private bool IsConfigValid = false;
        private IEnumerable<XElement> clogNodes = Enumerable.Empty<XElement>().ToList();
        private LibSource clogSource;
        private bool testFoldoutSaveState;
        private bool targetsFoldoutSaveState;
        private bool xmlFoldoutSaveState;
        private AnimBool testFoldoutState = new AnimBool(false);
        private AnimBool targetsFoldoutState = new AnimBool(true);
        private AnimBool xmlFoldoutState = new AnimBool(false);
        private string storedConfig;
        private Rect targetMenuRect = new Rect();
        private LogLevels testLogLevels = LogLevels.Everything;
        private IEnumerable<XElement> targetsXElements;
        private Dictionary<AdapterAttribute, Type> adapterTypeByAttribute;
        private IEnumerable<XElement> targetXElements;
        private IEnumerable<XElement> rootTargetXElements;
        private GameObject testContext;
        private string testLoggerName = "MyLoggerName";
        private string testMessage = "CLog test message";
        private bool testHasException;
        private XDocument xDocument;
        private string xmlEditorText;
        private PluginImporter clogImporter;
        private IEnumerable<PluginImporter> aiUnityImporters;
        private IEnumerable<PluginImporter> adapterImporters;
        private Vector2 scrollPos = Vector2.zero;
        private List<bool> targetFoldoutSaveStates;
        private HashSet<string> IncludeDefines;
        private HashSet<string> ExcludeDefines;
        private PlatformEnumFlagWrapper<BuildTargetGroup> buildTargetGroupFlagWrapper;

        #endregion

        #region private properties
        /// <summary>
        /// Internal logger singleton
        /// </summary>
        private static IInternalLogger Logger { get { return CLogInternalLogger.Instance; } }

        /// <summary>
        /// Gets a value indicating whether this instance is c log DLL.
        /// </summary>
        private bool IsCLogDll {
            get { return this.clogSource == LibSource.Dll; }
        }

        /// <summary>
        /// Gets the clog node.
        /// </summary>
        private XElement CLogNode {
            get { return this.clogNodes.FirstOrDefault(); }
        }

        /// <summary>
        /// Gets the targets XElement.
        /// </summary>
        private XElement TargetsXElement {
            get { return this.targetsXElements.FirstOrDefault(); }
        }

        #endregion

        #region private methods
        /// <summary>
        /// Unity Menu entry to launch clog editor control window.
        /// </summary>
        [MenuItem("Tools/AiUnity/CLog/Control Panel")]
        private static void ControlPanelMenu()
        {
            EditorWindow.GetWindow<CLogEditor>("CLog");
        }

        /// <summary>
        /// Unity Menu entry to launch AiUnity forums website.
        /// </summary>
        [MenuItem("Tools/AiUnity/CLog/Forums")]
        private static void ForumsMenu()
        {
            Application.OpenURL("https://forum.aiunity.com/categories");
        }

        /// <summary>
        /// Unity Menu entry to launch clog help website.
        /// </summary>
        [MenuItem("Tools/AiUnity/CLog/Help")]
        private static void HelpMenu()
        {
            Application.OpenURL("http://aiunity.com/products/clog");
        }

        /// <summary>
        /// Implicitly called by Unity for post serialization initialization
        /// </summary>
        private void OnEnable()
        {
            name = "CLogConfigWindow";
            PlayerPrefs.SetInt("AiUnityIsProSkin", Convert.ToInt32(EditorGUIUtility.isProSkin));

            // Used to hold GUI foldout state
            this.testFoldoutState = new AnimBool(this.testFoldoutSaveState);
            this.targetsFoldoutState = new AnimBool(this.targetsFoldoutSaveState);
            this.xmlFoldoutState = new AnimBool(this.xmlFoldoutSaveState);

            // Repaint GUI as foldout animated boolean changes to produce smooth visual effect.
            this.testFoldoutState.valueChanged.AddListener(Repaint);
            this.targetsFoldoutState.valueChanged.AddListener(Repaint);
            this.xmlFoldoutState.valueChanged.AddListener(Repaint);

            this.clogNodes = Enumerable.Empty<XElement>().ToList();
            this.foldoutStates = new Dictionary<XElement, AnimBool>();
            this.showAdvancedTarget = new Dictionary<XElement, AnimBool>();
            this.adapterTypeByAttribute = new Dictionary<AdapterAttribute, Type>();
            this.buildTargetGroupFlagWrapper = new PlatformEnumFlagWrapper<BuildTargetGroup>();
            this.storedConfig = string.Empty;
            this.IsConfigLoaded = false;
            this.IsConfigValid = false;
            this.xDocument = null;

            this.IncludeDefines = new HashSet<string>();
            this.ExcludeDefines = new HashSet<string>();

            SetupPlatforms();
            SetupImporters();
            ReflectAssembly();
            CheckConfig();
        }

        /// <summary>
        /// Check for the existence of a single CLog configuration file
        /// </summary>
        void CheckConfig()
        {
            string[] guids = AssetDatabase.FindAssets(CLogConfigFile.Instance.NameWithoutExtension + " t:TextAsset", null);
            IEnumerable<string> ConfigNames = guids.Select(g => AssetDatabase.GUIDToAssetPath(g)).Where(p => p.Contains("Resources") && p.EndsWith("xml"));

            if (ConfigNames.Count() > 1)
            {
                Logger.Error("Multiple CLog config files found under \"Resources\" directories:" + Environment.NewLine + string.Join(Environment.NewLine, ConfigNames.ToArray()));
            }
        }

        /// <summary>
        /// Implicitly called by Unity to draw Window GUI
        /// </summary>
        private void OnGUI()
        {
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.Space();
            EditorGUIUtility.labelWidth = 100f;

            DrawConfigGUI();
            DrawEditorSeparator();

            DrawTesterGUI();
            DrawEditorSeparator();
            this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, false, false);

            if (this.xDocument != null)
            {
                EditorGUI.BeginDisabledGroup(!this.IsConfigValid);

                DrawTargetsGUI();
                DrawEditorSeparator();

                EditorGUI.EndDisabledGroup();
            }

            if (this.IsConfigLoaded)
            {
                DrawXmlViewerGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draw the Configuration section of the editor GUI
        /// </summary>
        private void DrawConfigGUI()
        {
            // Refresh Config FileInfo in case it has been altered externally
            CLogConfigFile.Instance.FileInfo.Refresh();

            // Create config GUI
            EditorGUILayout.BeginHorizontal();
            GUIContent configFileLabel = new GUIContent("Config File", ConfigTooltip);
            EditorGUI.BeginDisabledGroup(true);
            int configWidth = Math.Max(350, CLogConfigFile.Instance.RelativeNameWithoutExtension.Length * 10); //nnn
            EditorGUILayout.TextField(configFileLabel, CLogConfigFile.Instance.RelativeNameWithoutExtension, GUILayout.MinWidth(configWidth));
            EditorGUI.EndDisabledGroup();

            // Mark config as not loaded if file is missing
            if (!CLogConfigFile.Instance.FileInfo.Exists)
            {
                this.IsConfigLoaded = false;
                this.IsConfigValid = false;
                this.xDocument = null;
            }
            else if (!this.IsConfigLoaded)
            {
                LoadParseXML();
            }

            // Create option to create config file if not loaded
            EditorGUI.BeginDisabledGroup(this.IsConfigLoaded);
            if (GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle))
            {
                LoadParseXML(DefaultConfig);
                SaveXML();
            }
            EditorGUI.EndDisabledGroup();

            // Create option to delete config file if loaded
            EditorGUI.BeginDisabledGroup(!this.IsConfigLoaded);
            if (GUILayout.Button(string.Empty, CustomEditorStyles.MinusIconStyle))
            {
                DeleteXML();
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/clog/manual#gui-config-file");
            }
            EditorGUILayout.EndHorizontal();

            if (!this.IsConfigLoaded)
            {
                EditorGUILayout.HelpBox("To configure CLog create a CLog.xml configuration file by clicking the + sign above.  The GUI controls below will then allow you to fully customize CLog.  In the absence of a configuration file the CLog framework will log Fatal, Error, and Warning messages with a default layout to UnityConsole.  Note the CLog framework is solely configured by reading the configuration XML resource file at runtime, leaving it completely independent from the Unity Editor.", MessageType.Info);
            }

            EditorGUI.BeginChangeCheck();

            // Create config source GUI
            GUIContent clogSourceContent = new GUIContent("Source", SourceTooltip);
            this.clogSource = (LibSource)EditorGUILayout.EnumPopup(clogSourceContent, this.clogSource);

            // Create config platforms GUI
            GUIContent buildPlatformsContent = new GUIContent("Platforms", PlatformsTooltip);
            this.buildTargetGroupFlagWrapper.EnumFlags = EditorGUILayout.MaskField(buildPlatformsContent, this.buildTargetGroupFlagWrapper.EnumFlags, this.buildTargetGroupFlagWrapper.EnumNames.ToArray());
            this.buildTargetGroupFlagWrapper.Add(BuildTargetGroup.Standalone);

            // Create config build log levels GUI
            GUIContent buildLevelsContent = new GUIContent("Build levels", BuildLevelsTooltip);
            IEnumerable<string> buildLevelDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Split(';')
                .Select(d => d.Trim()).Where(d => d.StartsWith("CLOG_")).Select(d => d.Substring(5).Replace("ALL", "EVERYTHING").ToLower().UppercaseLetter())
                .Where(d => Enum.GetNames(typeof(LogLevels)).Contains(d));
            LogLevels buildLevelsImport = string.Join(", ", buildLevelDefines.ToArray()).ToEnum<LogLevels>();
            LogLevels buildLevels = (LogLevels)EditorGUILayout.EnumFlagsField(buildLevelsContent, buildLevelsImport);

            // Source switch requires updates to dll plugin importer and preprocessor defines
            if (EditorGUI.EndChangeCheck() || AutoSourceDetect())
            {
                Logger.Info("CLog initiating recompile due to changes in DLL importers and Preprocessor defines.");

                // Save XML changes which would be lost due to recompile
                SaveXML();

                // Enable/Disable CLog PlugIn based upon settings
                foreach (PluginImporter pluginImporter in this.aiUnityImporters)
                {
                    if (pluginImporter.GetCompatibleWithAnyPlatform() != IsCLogDll || pluginImporter.GetCompatibleWithEditor() != IsCLogDll)
                    {
                        Logger.Info("Setting {0} plugin enable={1}", pluginImporter.assetPath, IsCLogDll);
                        try
                        {
                            pluginImporter.SetCompatibleWithAnyPlatform(IsCLogDll);
                            pluginImporter.SetCompatibleWithEditor(IsCLogDll);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to configure DLL importer={0}", pluginImporter.assetPath);
                        }
                    }
                }

                // Enable/Disable PlugIns based upon settings and dependencies
                foreach (var adapterImporter in this.adapterImporters)
                {
                    string targetDll = Path.GetFileName(adapterImporter.assetPath).Replace("Adapter", string.Empty);
                    PluginImporter targetImporter = PluginImporter.GetAllImporters().FirstOrDefault(p => p.assetPath.EndsWith(targetDll));
                    bool enableAdapter = IsCLogDll && targetImporter != null && targetImporter.GetCompatibleWithAnyPlatform();
                    adapterImporter.SetCompatibleWithAnyPlatform(enableAdapter);
                }
                //buildTargetGroupFlagWrapper.EnumValues
                foreach (BuildTargetGroup buildTargetGroup in this.buildTargetGroupFlagWrapper.EnumValues.Where(e => e != BuildTargetGroup.Unknown))
                {
                    List<string> initialDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';').Select(d => d.Trim()).Where(d => d != string.Empty).ToList();
                    List<string> revisedDefines = new List<string>(initialDefines);

                    // Purge existing CLog preprocessor defines
                    foreach (string define in initialDefines)
                    {
                        if (define.Equals("AIUNITY_CODE") || define.StartsWith("CLOG_") || this.ExcludeDefines.Contains(define))
                        {
                            revisedDefines.Remove(define);
                        }
                    }

                    // Establish CLOG_<Level> preprocessor define to indicate log levels enabled.
                    if (this.buildTargetGroupFlagWrapper.Has(buildTargetGroup))
                    {
                        // Establish AIUNITY_CODE preprocessor define to indicate if Source Code active
                        if (!IsCLogDll)
                        {
                            revisedDefines.Add("AIUNITY_CODE");
                        }

                        // Add custom defines that is based of GUI selections
                        revisedDefines.AddRange(this.IncludeDefines);

                        if (buildLevels == LogLevels.Everything)
                        {
                            revisedDefines.Add("CLOG_ALL");
                        }
                        else
                        {
                            foreach (LogLevels globalLevel in buildLevels.GetFlags())
                            {
                                revisedDefines.Add("CLOG_" + globalLevel.ToString().ToUpper());
                            }
                        }
                    }

                    if (!Enumerable.SequenceEqual(initialDefines.OrderBy(t => t), revisedDefines.OrderBy(t => t)))
                    {
                        try
                        {
                            // Persist preprocessor defines to Unity
                            Logger.Debug("Updated {0} defines New={1}{3}Old={2}", buildTargetGroup.ToString(), string.Join(";", revisedDefines.ToArray()), string.Join(";", initialDefines.ToArray()), Environment.NewLine);
                            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", revisedDefines.ToArray()));
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, "Failed to set preprocessor defines for platform={0}.", buildTargetGroup);
                        }
                    }
                }

                foreach (PluginImporter pluginImporter in this.aiUnityImporters)
                {
                    pluginImporter.SaveAndReimport();
                }
            }

            GUIContent internalLevelsContent = new GUIContent("Internal levels", InternalLevelsTooltip);
            Logger.InternalLogLevel = (LogLevels)EditorGUILayout.EnumFlagsField(internalLevelsContent, Logger.InternalLogLevel);

            // Create assert raise exception GUI
            bool result = false;
            GUIContent assertExceptionContent = new GUIContent("Assert raise", "Determine if failing CLog assertions should raise an exception.");
            XAttribute assertExceptionAttribute = CLogNode != null ? CLogNode.Attribute("assertException") : null;
            bool assertExceptionImport = assertExceptionAttribute != null && bool.TryParse(assertExceptionAttribute.Value, out result) && result;
            bool assertException = EditorGUILayout.Toggle(assertExceptionContent, assertExceptionImport);
            UpdateAttribute(CLogNode, "assertException", assertException.ToString(), "False");

            if (this.buildTargetGroupFlagWrapper.EnumFlags == buildTargetGroupFlagWrapper.EnumValueToFlag[BuildTargetGroup.Standalone])
            {
                EditorGUILayout.HelpBox("With \"Platforms\" set to Standalone, log messages on other platforms will be compile out of design.", MessageType.Info);
            }
            if (buildLevels == 0)
            {
                EditorGUILayout.HelpBox("With \"Build levels\" set to Nothing, all log messages will be compiled out of design.", MessageType.Info);
            }
        }

        /// <summary>
        /// Draw the Tester section of the editor GUI
        /// </summary>
        /// <exception cref="System.Exception">
        /// Inner Exception test message.
        /// or
        /// Outer Exception test message.
        /// </exception>
        private void DrawTesterGUI()
        {
            // Create test logger foldout GUI
            EditorGUILayout.BeginHorizontal();
            GUIContent testFoldoutContent = new GUIContent(string.Format("Test Logger ({0})", this.testLoggerName), "Use the test logger to validate your configuration");
            GUIStyle testStyle = new GUIStyle(EditorStyles.foldout);
            testStyle.margin.right = (int)testStyle.CalcSize(testFoldoutContent).x - 45;
            testStyle.stretchWidth = false;

            this.testFoldoutState.target = EditorGUILayout.Foldout(this.testFoldoutState.target, testFoldoutContent, testStyle);

            // Create test logger play GUI
            Texture2D playImage = EditorGUIUtility.Load(EditorGUIUtility.isProSkin ? "d_PlayButton" : "PlayButton") as Texture2D;
            GUIStyle playStyle = new GUIStyle(EditorStyles.miniButton) { padding = new RectOffset(1, 1, 1, 1) };

            // Create and run test log messages upon user request
            if (GUILayout.Button(playImage, playStyle, GUILayout.Width(20), GUILayout.Height(15)))
            {
                SaveXML();
                CLogger testLogger = CLogManager.Instance.GetLogger(this.testLoggerName, this.testContext);

                Exception testException = null;
                if (this.testHasException)
                {
                    try { throw new Exception("Inner Exception test message."); }
                    catch (Exception innerException)
                    {
                        try { throw new Exception("Outer Exception test message.", innerException); }
                        catch (Exception outerException) { testException = outerException; }
                    }
                }

                if (this.testLogLevels.Has(LogLevels.Assert))
                {
                    testLogger.Assert(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Fatal))
                {
                    testLogger.Fatal(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Error))
                {
                    testLogger.Error(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Warn))
                {
                    testLogger.Warn(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Info))
                {
                    testLogger.Info(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Debug))
                {
                    testLogger.Debug(testException, this.testMessage);
                }
                if (this.testLogLevels.Has(LogLevels.Trace))
                {
                    testLogger.Trace(testException, this.testMessage);
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/clog/manual#gui-test-logger");
            }
            EditorGUILayout.EndHorizontal();

            // Fade section of Test Logger GUI section
            if (EditorGUILayout.BeginFadeGroup(this.testFoldoutState.faded))
            {
                EditorGUI.indentLevel++;

                // Create test logger preview GUI
                List<string> logCommands = new List<string>();
                foreach (LogLevels testLogLevelFlag in this.testLogLevels.GetFlags())
                {
                    string assertArgument = testLogLevelFlag == LogLevels.Assert ? "false, " : string.Empty;
                    logCommands.Add(string.Format("logger.{0}({1}\"{2}\")", testLogLevelFlag, assertArgument, this.testMessage));
                }
                if (logCommands.Any())
                {
                    GUIStyle PreviewStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
                    EditorGUILayout.LabelField("Preview", string.Join(Environment.NewLine, logCommands.ToArray()), PreviewStyle);
                    EditorGUILayout.Space();
                }

                // Create test logger configuration parameter GUIs
                GUIContent testLoggerNameContent = new GUIContent("Name", "Name of logger to be tested.  In code the logger name can be set at logger instantiation or defaults to class name.");
                this.testLoggerName = EditorGUILayout.TextField(testLoggerNameContent, this.testLoggerName);
                GUIContent testContextContent = new GUIContent("Context", "GameObject associated with log message (Optional).  The gameObject gains focus when console log message is double clicked.");
                this.testContext = EditorGUILayout.ObjectField(testContextContent, this.testContext, typeof(GameObject), true) as GameObject;
                GUIContent testLevelsContent = new GUIContent("Levels", "A test log statement is generated for each selected level.");
                this.testLogLevels = (LogLevels)EditorGUILayout.EnumFlagsField(testLevelsContent, this.testLogLevels);
                GUIContent testMessageContent = new GUIContent("Message", "The message body of the test log statement.");
                this.testMessage = EditorGUILayout.TextField(testMessageContent, this.testMessage);
                GUIContent testHasExceptionContent = new GUIContent("Exception", "Add a test exception to the logging statements.");
                this.testHasException = EditorGUILayout.Toggle(testHasExceptionContent, this.testHasException);

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            // Transform foldout State to a simple bool list that can be serialized
            this.testFoldoutSaveState = this.testFoldoutState.value;
        }

        /// <summary>
        /// Draw the Targets section of the editor GUI
        /// </summary>
        private void DrawTargetsGUI()
        {
            // Create targets foldout GUI
            EditorGUILayout.BeginHorizontal();
            GUIContent targetsFoldoutContent = new GUIContent("Targets");
            GUIStyle targetsStyle = new GUIStyle(EditorStyles.foldout);
            targetsStyle.margin.right = (int)targetsStyle.CalcSize(targetsFoldoutContent).x - 45;
            targetsStyle.stretchWidth = false;

            this.targetsFoldoutState.target = EditorGUILayout.Foldout(this.targetsFoldoutState.target, targetsFoldoutContent, targetsStyle);
            bool addTarget = GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle);

            // During GUI repaint phase find and use location of last control to place generic menu
            if (Event.current.type == EventType.Repaint)
            {
                this.targetMenuRect = GUILayoutUtility.GetLastRect();
            }
            if (addTarget)
            {
                GenericMenu targetMenu = CreateTargetMenu();
                targetMenu.DropDown(this.targetMenuRect);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/clog/manual#targets");
            }
            EditorGUILayout.EndHorizontal();

            // Fade section of Targets GUI section
            if (EditorGUILayout.BeginFadeGroup(this.targetsFoldoutState.faded))
            {
                DrawTargetGUI(this.rootTargetXElements.ToList());
            }
            EditorGUILayout.EndFadeGroup();

            // Transform foldoutStates to a simple bool list that can be serialized
            this.targetsFoldoutSaveState = this.targetsFoldoutState.value;
            this.targetFoldoutSaveStates = this.foldoutStates.Where(x => x.Key.Name.LocalName.Equals("target")).Select(x => x.Value.value).ToList();
        }

        /// <summary>
        /// Draws the target GUI.
        /// </summary>
        /// <param name="targetXElements">The target x elements.</param>
        private void DrawTargetGUI(List<XElement> targetXElements)
        {
            int targetBaseIndexLevel = EditorGUI.indentLevel;

            foreach (XElement targetElement in targetXElements.ToList())
            {

                // Fade target when parent state is faded
                XAttribute typeAttribute = targetElement.GetOrSetAttribute("type", "UnityConsole");
                XAttribute nameAttribute = targetElement.GetOrSetAttribute("name", typeAttribute.Value);

                // EditorGUI.indentLevel = targetBaseIndexLevel + targetElement.AncestorsAndSelf(DefaultNamespace + "target").Count();
                EditorGUI.indentLevel = targetElement.AncestorsAndSelf(this.DefaultNamespace + "target").Count();

                // Create target foldout GUI
                EditorGUILayout.BeginHorizontal();
                GUIStyle targetStyle = new GUIStyle(EditorStyles.foldout);
                GUIContent targetFoldoutContent = new GUIContent(string.Format("<target name={0} type={1}>", nameAttribute.Value, typeAttribute.Value));
                targetStyle.margin.right = (int)targetStyle.CalcSize(targetFoldoutContent).x + (12 * EditorGUI.indentLevel) - 40;
                targetStyle.stretchWidth = false;

                this.foldoutStates[targetElement].target = EditorGUILayout.Foldout(this.foldoutStates.GetOrAdd(targetElement).target, targetFoldoutContent, targetStyle);

                if (this.adapterTypeByAttribute.Any(p => p.Key.IsWrapper))
                {
                    bool addSubTarget = GUILayout.Button(string.Empty, CustomEditorStyles.PlusIconStyle);

                    // During GUI repaint phase find and use location of last control to place generic menu
                    if (Event.current.type == EventType.Repaint)
                    {
                        this.targetMenuRect = GUILayoutUtility.GetLastRect();
                    }
                    if (addSubTarget)
                    {
                        GenericMenu targetMenu = CreateTargetMenu(targetElement);
                        targetMenu.DropDown(this.targetMenuRect);
                    }
                }


                if (GUILayout.Button(string.Empty, CustomEditorStyles.MinusIconStyle))
                {
                    RemoveFoldoutXElement(targetElement);
                    continue;
                    //return;
                }
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(string.Empty, CustomEditorStyles.CSScriptIconStyle))
                {
                    string guid = AssetDatabase.FindAssets(typeAttribute.Value).FirstOrDefault();
                    if (!string.IsNullOrEmpty(guid))
                    {
                        string assetPath = Application.dataPath.Remove(Application.dataPath.Length - 6) + AssetDatabase.GUIDToAssetPath(guid);
                        InternalEditorUtility.OpenFileAtLineExternal(assetPath, 0);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Missing Asset", string.Format("Unable to find \"{0}\" script.", typeAttribute.Value), "Ok");
                    }
                }

                // Create GUI control to move target up
                EditorGUI.BeginDisabledGroup(!(targetElement.PreviousNode is XElement));
                if (GUILayout.Button("Up", EditorStyles.miniButton))
                {
                    targetElement.MoveElementUp();
                }
                EditorGUI.EndDisabledGroup();

                // Create GUI control to move target down
                EditorGUI.BeginDisabledGroup(!(targetElement.NextNode is XElement));
                if (GUILayout.Button("Down", EditorStyles.miniButton))
                {
                    targetElement.MoveElementDown();
                }
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
                {
                    string anchor = Regex.Replace(typeAttribute.Value, @"(?<=.)([A-Z])", @"-$1").ToLower();
                    Application.OpenURL("http://aiunity.com/products/clog/manual#" + anchor);
                }

                EditorGUILayout.EndHorizontal();

                // Fade section of Target GUI section
                if (EditorGUILayout.BeginFadeGroup(this.foldoutStates[targetElement].faded))
                {
                    EditorGUI.indentLevel++;
                    IEnumerable<PropertyInfo> targetPropertyInfos = GetTargetPropertyInfo(typeAttribute.Value).Where(p => p.GetAttribute<DisplayAttribute>() != null).OrderBy(p => p.DeclaringType.GetInheritanceDepth()).ThenBy(p => p.GetAttribute<DisplayAttribute>().Order).ThenBy(p => p.GetAttribute<DisplayAttribute>().DisplayName).ThenBy(p => p.Name).ToList();

                    // Increase labelWidth if a target property name exceeds standard label space
                    int maxPropertyLength = targetPropertyInfos
                        .Select(p => (int)targetStyle.CalcSize(new GUIContent(p.GetAttribute<DisplayAttribute>().DisplayName ?? p.Name)).x).DefaultIfEmpty(0).Max();

                    float labelOffset = Math.Min(Math.Max(100f, maxPropertyLength), 150f);
                    EditorGUIUtility.labelWidth = labelOffset + EditorGUI.indentLevel * 20;

                    // Create target name GUI
                    GUIContent nameContent = new GUIContent("Name", "Name used to reference this target.");
                    string nameAttributeValue = EditorGUILayout.TextField(nameContent, nameAttribute.Value, GUILayout.ExpandWidth(true));
                    UpdateAttribute(targetElement, "name", nameAttributeValue);

                    // Create target type GUI
                    EditorGUI.BeginDisabledGroup(true);
                    GUIContent typeContent = new GUIContent("Type", "Immutable type of this target.");
                    typeAttribute.Value = EditorGUILayout.TextField(typeContent, typeAttribute.Value, GUILayout.ExpandWidth(true));
                    EditorGUI.EndDisabledGroup();

                    // Create target platforms GUI
                    /*XAttribute runPlatformAttribute = targetElement.Attribute("platforms");
                    RuntimePlatforms defaultRunPlatforms = RuntimePlatforms.Everything;
                    RuntimePlatforms runPlatforms = runPlatformAttribute != null ? runPlatformAttribute.Value.ToEnum<RuntimePlatforms>() : defaultRunPlatforms;
                    GUIContent platformsContent = new GUIContent("Platforms", "Platforms that will be logged by this target.");
                    runPlatforms = (RuntimePlatforms)EditorGUILayout.EnumFlagsField(platformsContent, runPlatforms);
                    string platformsUpdate = runPlatforms.Equals(RuntimePlatforms.Everything) ? "Everything" : string.Join(", ", runPlatforms.GetFlags().Select(p => p.ToString()).ToArray());
                    UpdateAttribute(targetElement, "platforms", platformsUpdate, defaultRunPlatforms.ToString());*/
                    XAttribute runPlatformAttribute = targetElement.Attribute("platforms");
                    PlatformEnumFlagWrapper<RuntimePlatform> runtimePlatformFlagWrapper = runPlatformAttribute != null ? runPlatformAttribute.Value : "Everything";
                    GUIContent platformsContent = new GUIContent("Platforms", "Platforms that will be logged by this target.");
                    runtimePlatformFlagWrapper.EnumFlags = EditorGUILayout.MaskField(platformsContent, runtimePlatformFlagWrapper.EnumFlags, runtimePlatformFlagWrapper.EnumNames.ToArray());
                    UpdateAttribute(targetElement, "platforms", runtimePlatformFlagWrapper.ToString(), "Everything");

                    //EditorGUILayout.Space();
                    IEnumerable<PropertyInfo> basicPropertyInfos = targetPropertyInfos.Where(p => !p.GetAttribute<DisplayAttribute>().Advanced);
                    DrawTargetProperties(targetElement, basicPropertyInfos);

                    IEnumerable<PropertyInfo> advancedPropertyInfos = targetPropertyInfos.Where(p => p.GetAttribute<DisplayAttribute>().Advanced);
                    if (advancedPropertyInfos.Any())
                    {
                        GUIContent advanceContent = new GUIContent("Advance options");
                        GUIStyle advanceStyle = new GUIStyle(EditorStyles.foldout);
                        bool showFoldout = this.showAdvancedTarget.GetOrAdd(targetElement, new AnimBool(true)).target;
                        this.showAdvancedTarget[targetElement].target = EditorGUILayout.Foldout(showFoldout, advanceContent, advanceStyle);

                        if (EditorGUILayout.BeginFadeGroup(this.showAdvancedTarget[targetElement].faded))
                        {
                            DrawTargetProperties(targetElement, advancedPropertyInfos);
                        }
                        EditorGUILayout.EndFadeGroup();
                    }

                    if (targetElement.Elements().Any())
                    {
                        EditorGUILayout.Space();
                        //DrawTargetGUI(targetElement.Elements().ToList());
                        DrawTargetGUI(targetElement.Elements(this.DefaultNamespace + "target").ToList());
                    }
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUI.indentLevel = targetBaseIndexLevel;
            }
        }

        /// <summary>
        /// Draws the target properties.
        /// </summary>
        /// <param name="targetElement">The target element.</param>
        /// <param name="propertyInfos">The property infos.</param>
        private void DrawTargetProperties(XElement targetElement, IEnumerable<PropertyInfo> propertyInfos)
        {
            Type declareType = null;

            // Create target customer editor controls by reflecting over target type properties
            foreach (PropertyInfo targetPropertyInfo in propertyInfos)
            {
                if (declareType != null && targetPropertyInfo.DeclaringType != declareType)
                {
                    EditorGUILayout.Space();
                }
                declareType = targetPropertyInfo.DeclaringType;

                string targetPropertyName = targetPropertyInfo.Name.LowercaseLetter();
                string targetDisplayName = targetPropertyInfo.GetAttribute<DisplayAttribute>().DisplayName ?? targetPropertyName;

                XAttribute targetXAttribute = targetElement.Attribute(targetPropertyName);
                string defaultXAttributeValue = targetPropertyInfo.GetAttributes<DefaultValueAttribute>().Select(v => v.Value.ToString()).DefaultIfEmpty(string.Empty).FirstOrDefault();
                string targetXAttributeValue = targetXAttribute != null ? targetXAttribute.Value : defaultXAttributeValue;

                GUIContent targetContent = new GUIContent(targetDisplayName, targetPropertyInfo.GetAttribute<DisplayAttribute>().Tooltip);
                targetXAttributeValue = CreateTypeBasedGUI(targetPropertyInfo.PropertyType, targetXAttributeValue, targetContent);
                UpdateAttribute(targetElement, targetPropertyName, targetXAttributeValue, defaultXAttributeValue);
            }
        }

        /// <summary>
        /// Draw the XML Viewer section of the editor GUI
        /// </summary>
        private void DrawXmlViewerGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUIStyle xmlStyle = new GUIStyle(EditorStyles.foldout);
            xmlStyle.onNormal.textColor = this.IsConfigValid ? EditorStyles.foldout.onNormal.textColor : Color.red;

            this.xmlFoldoutState.target = EditorGUILayout.Foldout(this.xmlFoldoutState.target, "XML Viewer", xmlStyle);

            if (GUILayout.Button(string.Empty, CustomEditorStyles.HelpIconStyle))
            {
                Application.OpenURL("http://aiunity.com/products/clog/manual#gui-xml-viewer");
            }
            EditorGUILayout.EndHorizontal();

            // Fade section of XML Viewer GUI section
            if (EditorGUILayout.BeginFadeGroup(this.xmlFoldoutState.faded))
            {
                if (this.IsConfigValid && !GUI.GetNameOfFocusedControl().Equals("XMLViewer"))
                {
                    this.xmlEditorText = this.xDocument.ToString();
                }

                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName("XMLViewer");
                this.xmlEditorText = EditorGUILayout.TextArea(this.xmlEditorText);
                if (EditorGUI.EndChangeCheck())
                {
                    LoadParseXML(this.xmlEditorText);
                }
            }
            EditorGUILayout.EndFadeGroup();

            // Transform foldout State to a simple bool list that can be serialized
            this.xmlFoldoutSaveState = this.xmlFoldoutState.value;
        }


        /// <summary>
        /// Called by Unity Internals when windows loses focus
        /// </summary>
        private void OnLostFocus()
        {
            SaveXML();
        }

        /// <summary>
        /// Create and Add the states of the foldout to XElement indexed Dictionary with initial states.
        /// </summary>
        /// <param name="foldoutXElements">Foldout X elements.</param>
        /// <param name="initialStates">The initial states.</param>
        private void AddFoldoutState(IEnumerable<XElement> foldoutXElements, IEnumerable<bool> initialStates = null)
        {
            int index = 0;

            foreach (XElement foldoutXElement in foldoutXElements)
            {
                bool initialState = initialStates != null && initialStates.ElementAtOrDefault(index++);
                AddFoldoutState(foldoutXElement, initialState);
            }
        }

        /// <summary>
        /// Create and Add the state of the foldout to XElement indexed Dictionary with initial state.
        /// </summary>
        /// <param name="foldoutXElement">Foldout X element.</param>
        /// <param name="initialState">If set to <c>true</c> initial state.</param>
        private void AddFoldoutState(XElement foldoutXElement, bool initialState = false)
        {
            this.foldoutStates.Add(foldoutXElement, CreateRepaintAnimBool(initialState));
            this.showAdvancedTarget.Add(foldoutXElement, CreateRepaintAnimBool(false));
        }

        /// <summary>
        /// Adds a clog target.
        /// </summary>
        /// <param name="targetTypeName">Name of the target type.</param>
        /// <param name="referenceXElement">The reference x element.</param>
        /// <param name="child">if set to <c>true</c> [child].</param>
        private void AddTarget(string targetTypeName, XElement referenceXElement = null, bool child = true)
        {
            string targetName = targetTypeName;

            for (int i = 1; i < 10; i++)
            {
                if (!this.targetXElements.Any(x => x.Attributes("name").Any(a => a.Value == targetName)))
                {
                    break;
                }
                targetName = targetTypeName + i;
            }

            Logger.Debug("Add new target name = {0} type = {1}", targetName, targetTypeName);
            XElement targetXElement = new XElement(this.DefaultNamespace + "target", new XAttribute("name", targetName), new XAttribute("type", targetTypeName), new XAttribute("platforms", "Everything"), new XAttribute("logLevels", "Everything"));

            if (referenceXElement != null)
            {
                if (child)
                {
                    referenceXElement.Add(targetXElement);
                }
                else
                {
                    referenceXElement.AddAfterSelf(targetXElement);
                    referenceXElement.Remove();
                    targetXElement.Add(referenceXElement);
                }
            }
            else
            {
                if (TargetsXElement != null)
                {
                    TargetsXElement.Add(targetXElement);
                }
                else
                {
                    if (CLogNode != null)
                    {
                        CLogNode.Add(new XElement(this.DefaultNamespace + "targets"));
                        TargetsXElement.Add(targetXElement);
                    }
                }
            }
            AddFoldoutState(targetXElement);
        }

        /// <summary>
        /// Creates an animation bool that also triggers GUI Repaint.
        /// </summary>
        /// <param name="initialState">If set to <c>true</c> initial state.</param>
        /// <returns>The repaint animation bool.</returns>
        private AnimBool CreateRepaintAnimBool(bool initialState)
        {
            AnimBool animBool = new AnimBool(initialState);
            animBool.valueChanged.AddListener(Repaint);
            return animBool;
        }

        /// <summary>
        /// Creates the target menu used to display available targets.
        /// </summary>
        /// <param name="referenceXElement">The reference x element.</param>
        /// <returns>GenericMenu.</returns>
        private GenericMenu CreateTargetMenu(XElement referenceXElement = null)
        {
            GenericMenu targetMenu = new GenericMenu();

            bool isWrapperReference = referenceXElement != null && referenceXElement.Attribute("type") != null && this.adapterTypeByAttribute.Any(p => p.Key.IsWrapper && p.Key.DisplayName.Equals(referenceXElement.Attribute("type").Value));

            if (referenceXElement == null || isWrapperReference)
            {
                foreach (KeyValuePair<AdapterAttribute, Type> target in this.adapterTypeByAttribute.Where(p => !p.Key.IsWrapper && !p.Key.IsCompound))
                {
                    string closureTargetTypeName = target.Key.DisplayName;
                    targetMenu.AddItem(new GUIContent(target.Key.DisplayName), false, () => AddTarget(closureTargetTypeName, referenceXElement));
                }
                if (this.adapterTypeByAttribute.Any(p => p.Key.IsWrapper || p.Key.IsCompound))
                {
                    targetMenu.AddSeparator(string.Empty);
                }
            }

            foreach (KeyValuePair<AdapterAttribute, Type> target in this.adapterTypeByAttribute.Where(p => p.Key.IsWrapper && !p.Key.IsCompound))
            {
                string closureTargetTypeName = target.Key.DisplayName;
                targetMenu.AddItem(new GUIContent(target.Key.DisplayName), false, () => AddTarget(closureTargetTypeName, referenceXElement, false));
            }
            if (this.adapterTypeByAttribute.Any(p => p.Key.IsCompound))
            {
                targetMenu.AddSeparator(string.Empty);
            }

            foreach (KeyValuePair<AdapterAttribute, Type> target in this.adapterTypeByAttribute.Where(p => p.Key.IsCompound))
            {
                string closureTargetTypeName = target.Key.DisplayName;
                targetMenu.AddItem(new GUIContent(target.Key.DisplayName), false, () => AddTarget(closureTargetTypeName, referenceXElement, false));
            }

            return targetMenu;
        }

        /// <summary>
        /// Draw the Configuration section of the editor GUI
        /// </summary>
        private void DrawEditorSeparator()
        {
            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, CustomEditorStyles.EditorLine, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
            EditorGUILayout.Space();
        }

        /// <summary>
        /// Gets the property info for the specified type.
        /// </summary>
        /// <param name="targetTypeName">Name of the target type.</param>
        /// <returns>The target property info.</returns>
        private IEnumerable<PropertyInfo> GetTargetPropertyInfo(string targetTypeName)
        {
            Type adapterType = this.adapterTypeByAttribute.Where(p => p.Key.DisplayName.Equals(targetTypeName)).Select(p => p.Value).FirstOrDefault();

            if (adapterType == null)
            {
                if (this.adapterTypeByAttribute.Count != 0)
                {
                    Logger.Error("Unable to find CLog Target Type={0} as specified in {1}", targetTypeName, CLogConfigFile.Instance.FileInfo.FullName);
                }
                return Enumerable.Empty<PropertyInfo>();
            }
            return adapterType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
        }

        /// <summary>
        /// Load and parse the configuration XML file.
        /// </summary>
        private void LoadParseXML()
        {
            LoadXML();
            if (this.IsConfigValid)
            {
                ParseXML();
            }
        }

        /// <summary>
        /// Load and parse the configuration XML text.
        /// </summary>
        /// <param name="xmlText">Xml text.</param>
        private void LoadParseXML(string xmlText)
        {
            LoadXML(xmlText);
            if (this.IsConfigValid)
            {
                ParseXML();
            }
        }

        /// <summary>
        /// Load the configuration XML file.
        /// </summary>
        private void LoadXML()
        {
            TextAsset configXMLAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CLogConfigFile.Instance.RelativeName);
            string configText = configXMLAsset != null ? configXMLAsset.text : string.Empty;

            if (string.IsNullOrEmpty(configText))
            {
                this.IsConfigLoaded = false;
                this.IsConfigValid = false;
                this.xDocument = null;
            }
            else
            {
                try
                {
                    LoadXML(configText);

                    this.storedConfig = this.xDocument.ToString();
                    this.IsConfigLoaded = true;
                    this.IsConfigValid = true;
                }
                catch (XmlException xmlException)
                {

                    Logger.Error("Unable to load Config XML file = " + CLogConfigFile.Instance.RelativeName, xmlException);
                    this.IsConfigLoaded = true;
                    this.IsConfigValid = false;
                    this.xDocument = null;

                    this.storedConfig = this.xmlEditorText = configText;
                }
            }
        }

        /// <summary>
        /// Load the configuration XML text.
        /// </summary>
        /// <param name="xmlText">Xml text.</param>
        private void LoadXML(string xmlText)
        {
            try
            {
                this.xDocument = XDocument.Parse(xmlText);
                this.IsConfigValid = true;
            }
            catch (XmlException)
            {
                this.IsConfigValid = false;
            }
        }

        /// <summary>
        /// Parses the loaded config xDocument for elements pertaining to clog
        /// </summary>
        private void ParseXML()
        {
            if (this.xDocument != null && this.xDocument.Root != null && this.xDocument.Root.Name.LocalName.Equals("clog"))
            {
                this.clogNodes = this.xDocument.Root.Yield();
            }
            else
            {
                this.clogNodes = Enumerable.Empty<XElement>();
            }

            this.IsConfigValid = this.clogNodes.Count().Equals(1);
            this.DefaultNamespace = this.clogNodes.Select(e => e.GetDefaultNamespace()).DefaultIfEmpty(string.Empty).FirstOrDefault();

            this.targetsXElements = this.clogNodes.Descendants(this.DefaultNamespace + "targets");
            this.targetXElements = this.targetsXElements.Descendants(this.DefaultNamespace + "target");
            this.rootTargetXElements = this.targetsXElements.Elements(this.DefaultNamespace + "target");
            AddFoldoutState(this.targetXElements, this.targetFoldoutSaveStates);
        }

        /// <summary>
        /// Save the configuration XML file.
        /// </summary>
        private void SaveXML()
        {
            if (this.xDocument != null && this.IsConfigValid)
            {
                if (!this.xDocument.ToString().Equals(this.storedConfig))
                {
                    Logger.Debug("Saving XML File = {0}", CLogConfigFile.Instance.FileInfo.FullName);
                    this.xDocument.Save(CLogConfigFile.Instance.FileInfo.FullName);
                    this.storedConfig = this.xDocument.ToString();
                    AssetDatabase.Refresh();
                    CLogManager.Instance.Reset();
                }
            }
        }

        /// <summary>
        /// Delete the configuration XML file.
        /// </summary>
        private void DeleteXML()
        {
            this.storedConfig = string.Empty;
            this.IsConfigLoaded = false;
            this.IsConfigValid = false;
            this.xDocument = null;

            if (string.IsNullOrEmpty(CLogConfigFile.Instance.RelativeName))
            {
                Logger.Warn("Failed to delete XML File = {0}", CLogConfigFile.Instance.FileInfo.FullName);
            }
            else
            {
                Logger.Debug("Deleting XML File = {0}", CLogConfigFile.Instance.FileInfo.FullName);
                AssetDatabase.DeleteAsset(CLogConfigFile.Instance.RelativeName);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Determine build platforms based upon preprocessor defines
        /// </summary>
        private void SetupPlatforms()
        {
            try
            {
                // Determine current active platforms based upon preprocessor defines
                this.buildTargetGroupFlagWrapper = BuildTargetGroup.Standalone;
                foreach (BuildTargetGroup buildTargetGroup in this.buildTargetGroupFlagWrapper.EnumValues)
                {
                    //BuildTargetGroup buildTargetGroup = platformTarget.ToString().ToEnum<BuildTargetGroup>();
                    string[] defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';');
                    if (defines.Any(d => d.Contains("CLOG_")))
                    {
                        this.buildTargetGroupFlagWrapper.Add(buildTargetGroup);
                    }
                }
                /*if (EnumExtensions.GetValues<BuildPlatforms>().All(p => this.buildPlatforms.Has(p)))
                {
                    this.buildPlatforms = BuildPlatforms.Everything;
                }*/
            }
            catch
            {
                Logger.Error("Failed to get preprocessor defines on platform(s): ", string.Join(Environment.NewLine, this.buildTargetGroupFlagWrapper.EnumNames.ToArray()));
            }
        }

        /// <summary>
        /// Get CLog DLL plugin Importers
        /// </summary>
        private void SetupImporters()
        {
            // Load plugin importers related to CLog
            this.aiUnityImporters = PluginImporter.GetAllImporters().Where(p => p.assetPath.Contains("/AiUnity/"));
            this.clogImporter = this.aiUnityImporters.FirstOrDefault(p => p.assetPath.EndsWith("CLog.dll"));
            this.adapterImporters = this.aiUnityImporters.Where(p => p.assetPath.EndsWith("CLogAdapter.dll"));

            this.clogSource = this.clogImporter != null && (this.clogImporter.GetCompatibleWithEditor()) ? LibSource.Dll : LibSource.Code;
        }

        /// <summary>
        /// Automatically determines Source (DLL/Code) based upon plugin importer
        /// </summary>
        /// <returns><c>true</c> if source changed, <c>false</c> otherwise.</returns>
        private bool AutoSourceDetect()
        {
            bool aiUnityCodeDefined = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains("AIUNITY_CODE");

            // Switch to CLog source code if dll plugin importers cannot be located
            if (IsCLogDll)
            {
                if (aiUnityCodeDefined || this.clogImporter == null || (!this.clogImporter.GetCompatibleWithAnyPlatform() && !this.clogImporter.GetCompatibleWithEditor()))
                {
                    if (EditorUtility.DisplayDialog("CLog auto source switch", "Switching to source due to DLL importer and preprocessor defines settings.", "OK", "Cancel"))
                    {
                        this.clogSource = LibSource.Code;
                        return true;
                    }
                }
            }
            else if (!aiUnityCodeDefined || (this.clogImporter != null && (this.clogImporter.GetCompatibleWithAnyPlatform() || this.clogImporter.GetCompatibleWithEditor())))
            {
                if (EditorUtility.DisplayDialog("CLog auto source switch", "Switching to DLLs due to DLL importer and preprocessor defines settings.", "OK", "Cancel"))
                {
                    this.clogSource = LibSource.Dll;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Create Editor GUI that corresponds to propertyType
        /// </summary>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="targetXAttributeValue">The target x attribute value.</param>
        /// <param name="targetContent">Content of the target.</param>
        /// <returns>System.String.</returns>
        private string CreateTypeBasedGUI(Type propertyType, string targetXAttributeValue, GUIContent targetContent)
        {
            if (propertyType.IsEnum && !string.IsNullOrEmpty(targetXAttributeValue))
            {

                Enum enumValue = targetXAttributeValue.ToEnum(propertyType);
                try
                {
                    if (propertyType.GetCustomAttributes(typeof(FlagsAttribute), true).Any())
                    {
                        return EditorGUILayout.EnumFlagsField(targetContent, enumValue).ToString();
                    }
                    else
                    {
                        return EditorGUILayout.EnumPopup(targetContent, enumValue).ToString();
                    }
                }
                catch
                {
                    return string.Empty;
                }
            }
            else if (propertyType == typeof(Color))
            {
                Color color;
                ColorUtility.TryParseHtmlString(targetXAttributeValue, out color);
                return "#" + ColorUtility.ToHtmlStringRGBA(EditorGUILayout.ColorField(targetContent, color));
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(propertyType))
            {
                return string.Empty;
            }
            else
            {
                switch (Type.GetTypeCode(propertyType))
                {
                    case TypeCode.Boolean:
                        bool boolAttribute = false;
                        bool.TryParse(targetXAttributeValue, out boolAttribute);
                        return EditorGUILayout.Toggle(targetContent, boolAttribute).ToString();
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        int intAttribute;
                        int.TryParse(targetXAttributeValue, out intAttribute);
                        return EditorGUILayout.IntField(targetContent, intAttribute).ToString();
                    case TypeCode.Single:
                        float floatAttribute;
                        float.TryParse(targetXAttributeValue, out floatAttribute);
                        return EditorGUILayout.FloatField(targetContent, floatAttribute).ToString();
                    case TypeCode.Decimal:
                    case TypeCode.Double:
                        double doubleAttribute;
                        double.TryParse(targetXAttributeValue, out doubleAttribute);
                        return EditorGUILayout.DoubleField(targetContent, doubleAttribute).ToString();
                    case TypeCode.Empty:
                    case TypeCode.Object:
                    case TypeCode.DBNull:
                    case TypeCode.DateTime:
                    case TypeCode.Char:
                    case TypeCode.String:
                    default:
                        return EditorGUILayout.TextField(targetContent, targetXAttributeValue);
                }
            }
        }

        /// <summary>
        /// Reflects relevant assemblies to discover elements used by clog.
        /// </summary>
        private void ReflectAssembly()
        {
            List<string> searchAssemblyNames = new List<string>() { "Assembly-CSharp", Assembly.GetExecutingAssembly().GetName().Name, "CLog" };
            IEnumerable<Assembly> searchAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => searchAssemblyNames.Any(t => a.FullName.StartsWith(t)));
            IEnumerable<Type> adapterTypes = searchAssemblies.SelectMany(a => a.GetTypes()).Where(t => typeof(ILoggerFactoryAdapter).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (Type adapterType in adapterTypes)
            {
                AdapterAttribute adapterAttribute = adapterType.GetCustomAttributes(typeof(AdapterAttribute), true).FirstOrDefault() as AdapterAttribute;
                if (adapterAttribute != null)
                {
                    this.adapterTypeByAttribute[adapterAttribute] = adapterType;
                }
            }
        }

        /// <summary>
        /// Removes the state of the foldout.
        /// </summary>
        /// <param name="foldoutXElement">Foldout XElement.</param>
        private void RemoveFoldoutState(XElement foldoutXElement)
        {
            if (!this.foldoutStates.Remove(foldoutXElement))
            {
                Logger.Assert("Unable to Remove Target Foldout State");
            }
            if (!this.showAdvancedTarget.Remove(foldoutXElement))
            {
                Logger.Assert("Unable to Remove Target Advanced Foldout State");
            }
        }

        /// <summary>
        /// Removes the foldout XElement.
        /// </summary>
        /// <param name="targetXElement">Target XElement.</param>
        private void RemoveFoldoutXElement(XElement targetXElement)
        {
            Logger.Assert(targetXElement != null, "Unable to Remove Target XElement");

            if (targetXElement != null)
            {
                RemoveFoldoutState(targetXElement);
                targetXElement.Remove();
            }
        }

        /// <summary>
        /// Updates the attribute with checks.
        /// </summary>
        /// <param name="xElement">XElement.</param>
        /// <param name="xAttributeName">XAttribute name.</param>
        /// <param name="xAttributeValue">XAttribute value.</param>
        /// <param name="defaultXAttributeValue">The default x attribute value.</param>
        private void UpdateAttribute(XElement xElement, string xAttributeName, string xAttributeValue, string defaultXAttributeValue = null)
        {
            XAttribute xAttribute = xElement != null ? xElement.Attribute(xAttributeName) : null;

            if (xAttributeValue == null || xAttributeValue == defaultXAttributeValue)
            {
                if (xAttribute != null)
                {
                    xAttribute.Remove();
                }
            }
            else if (xAttribute == null)
            {
                if (xElement != null && xAttributeValue != null)
                {
                    xElement.Add(new XAttribute(xAttributeName, xAttributeValue));
                }
            }
            else
            {
                xAttribute.Value = xAttributeValue;
            }
        }
        #endregion
    }
}
