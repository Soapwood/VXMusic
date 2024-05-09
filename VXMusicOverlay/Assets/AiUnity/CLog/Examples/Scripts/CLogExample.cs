// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 02-17-2016
// Modified         : 12-19-2017
// ***********************************************************************
namespace AiUnity.CLog.Examples.Scripts
{
    using AiUnity.CLog.Core;
    using System;
    using UnityEngine;

    /// <summary>
    /// This file attaches to a GameObject and demonstrates the simplicity of creating CLog messages.
    /// The procedure involves getting a logger instance from CLogManger and then writing your log statements.
    /// By using these log messages in your code you gain immense power through the CLog XML configuration.
    /// This configuration is maintained by the comprehensive Unity Editor GUI Window located at
    /// Tools:AiUnity:CLog:Editor.  The CLog abstraction allows you to select your custom/supplied logger on
    /// a platform basis.  Please see the detailed documentation to learn more about CLog.
    /// 
    /// To see log messages please configure CLog GUI with the desired log level and message target.
    /// </summary>
    public class CLogExample : MonoBehaviour
    {
        private CLogger logger;
        public GameObject gameObjectContext;

        void Awake()
        {
            Debug.Log("Standard Unity Debug log message, called from CLogExample Awake() method.\nPlease configure NLog GUI with the desired target and message level.");

            // Create class logger and associate it with this class instance/name.
            this.logger = CLogManager.Instance.GetLogger(this);
            // Debug message that can be augmented, filtered, and routed by NLog GUI.
            this.logger.Info("Testing a CLog <i>Info</i> message from {0} Awake() method.", GetType().Name);
        }

        void Start()
        {
            int assertCondition = 1;

            // Basic logging statements
            this.logger.Assert(assertCondition == 0, "Testing a CLog <i>Assert</i> message from {0} Start() method.", GetType().Name);
            this.logger.Fatal("Testing a CLog <i>Fatal</i> message from {0} Start() method.", GetType().Name);
            this.logger.Error("Testing a CLog <i>Error</i> message from {0} Start() method.", GetType().Name);
            this.logger.Warn("Testing a CLog <i>Warn</i> message from {0} Start() method.", GetType().Name);
            this.logger.Info("Testing a CLog <i>Info</i> message from {0} Start() method.", GetType().Name);
            this.logger.Debug("Testing a CLog <i>Debug</i> message from {0} Start() method.", GetType().Name);
            this.logger.Trace("Testing a CLog <i>Trace</i> message from {0} Start() method.", GetType().Name);

            // Use the <context> argument to explicitly associate a message with a gameObject
            this.logger.Info(gameObjectContext, "Test CLog message with an explicit gameObject context");

            // Create a test exception for illustration.
            Exception testException;
            try { throw new Exception("Test Exception"); }
            catch (Exception exception) { testException = exception; }

            // Added exception will be formatted and appended to message
            this.logger.Info(testException, "Test CLog message with an exception");
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.Label("If messages missing on expected destination please configure CLog GUI with desired LogLevel (i.e. Everything) and target (i.e. UnityConsole/GameConsole).");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}