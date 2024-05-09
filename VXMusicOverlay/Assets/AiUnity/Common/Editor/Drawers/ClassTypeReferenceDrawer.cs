// ***********************************************************************
// Assembly         : Assembly-CSharp-Editor
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 01-11-2018
// ***********************************************************************
using AiUnity.Common.Attributes;
using AiUnity.Common.Serialization.ClassTypeReference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AiUnity.Common.Editor.Drawers
{
    /// <summary>
    /// Custom property drawer for <see cref="ClassTypeReference" /> properties.
    /// </summary>
    /// <seealso cref="UnityEditor.PropertyDrawer" />
    [CustomPropertyDrawer(typeof(ClassTypeReference))]
    [CustomPropertyDrawer(typeof(ClassTypeConstraintAttribute), true)]
    public sealed class ClassTypeReferenceDrawer : PropertyDrawer
    {
        #region Fields
        private static readonly int s_ControlHint = typeof(ClassTypeReferenceDrawer).GetHashCode();
        private static readonly GenericMenu.MenuFunction2 s_OnSelectedTypeName = OnSelectedTypeName;
        private static string s_SelectedClassRef;
        private static int s_SelectionControlID;
        private static GUIContent s_TempContent = new GUIContent();
        private static Dictionary<string, Type> s_TypeMap = new Dictionary<string, Type>();
        #endregion

        #region Methods
        /// <summary>
        /// Override this method to specify how tall the GUI for this field is in pixels.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        /// <returns>The height in pixels.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorStyles.popup.CalcHeight(GUIContent.none, 0);
        }


        /// <summary>
        /// Override this method to make your own GUI for the property.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var classTypeConstraintAttribute = attribute as ClassTypeConstraintAttribute;

            if (classTypeConstraintAttribute != null && classTypeConstraintAttribute.LabelContent != null)
            {
                label.text = classTypeConstraintAttribute.LabelContent.text;
                label.tooltip = classTypeConstraintAttribute.LabelContent.tooltip;
                label.image = classTypeConstraintAttribute.LabelContent.image;
            }
            //Debug.Log("Property=" + property);
            DrawTypeSelectionControl(position, property.FindPropertyRelative("_classRef"), label, classTypeConstraintAttribute);
        }

        /// <summary>
        /// Displays the drop down.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="types">The types.</param>
        /// <param name="selectedType">Type of the selected.</param>
        /// <param name="grouping">The grouping.</param>
        private static void DisplayDropDown(Rect position, List<Type> types, Type selectedType, ClassGrouping grouping)
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("(None)"), selectedType == null, s_OnSelectedTypeName, null);
            menu.AddSeparator(string.Empty);

            for (var i = 0; i < types.Count; ++i)
            {
                var type = types[i];

                var menuLabel = FormatGroupedTypeName(type, grouping);
                if (string.IsNullOrEmpty(menuLabel)) continue;
                var content = new GUIContent(menuLabel);
                menu.AddItem(content, type == selectedType, s_OnSelectedTypeName, type);
            }
            menu.DropDown(position);
        }

        /// <summary>
        /// Draws the type selection control.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="label">The label.</param>
        /// <param name="classRef">The class reference.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>System.String.</returns>
        private static string DrawTypeSelectionControl(Rect position, GUIContent label, string classRef, ClassTypeConstraintAttribute filter)
        {
            if (label != null && label != GUIContent.none) position = EditorGUI.PrefixLabel(position, label);
            var controlID = GUIUtility.GetControlID(s_ControlHint, FocusType.Keyboard, position);

            var triggerDropDown = false;

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.ExecuteCommand:
                    if (Event.current.commandName == "TypeReferenceUpdated")
                        if (s_SelectionControlID == controlID)
                        {
                            if (classRef != s_SelectedClassRef)
                            {
                                classRef = s_SelectedClassRef;
                                GUI.changed = true;
                            }

                            s_SelectionControlID = 0;
                            s_SelectedClassRef = null;
                        }
                    break;

                case EventType.MouseDown:
                    if (GUI.enabled && position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlID;
                        triggerDropDown = true;
                        Event.current.Use();
                    }
                    break;

                case EventType.KeyDown:
                    if (GUI.enabled && GUIUtility.keyboardControl == controlID)
                        if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Space)
                        {
                            triggerDropDown = true;
                            Event.current.Use();
                        }
                    break;

                case EventType.Repaint:
                    var classRefParts = classRef.Split(',');
                    var classFullName = classRefParts[0].Trim();
                    s_TempContent.text = classFullName.Substring(classFullName.LastIndexOf('.') + 1);
                    if (s_TempContent.text == string.Empty) s_TempContent.text = "(None)";
                    else if (ResolveType(classRef) == null) s_TempContent.text += " {Missing}";
                    EditorStyles.popup.Draw(position, s_TempContent, controlID);
                    break;
            }

            if (triggerDropDown)
            {
                s_SelectionControlID = controlID;
                s_SelectedClassRef = classRef;
                var filteredTypes = GetFilteredTypes(filter);
                DisplayDropDown(position, filteredTypes, ResolveType(classRef), filter.Grouping);
            }

            return classRef;
        }

        /// <summary>
        /// Draws the type selection control.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="property">The property.</param>
        /// <param name="label">The label.</param>
        /// <param name="filter">The filter.</param>
        private static void DrawTypeSelectionControl(Rect position, SerializedProperty property, GUIContent label, ClassTypeConstraintAttribute filter)
        {
            //Debug.Log("Relative Property=" + property);
            var restoreShowMixedValue = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            property.stringValue = DrawTypeSelectionControl(position, label, property.stringValue, filter);

            EditorGUI.showMixedValue = restoreShowMixedValue;
        }

        /// <summary>
        /// Filters the types.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="output">The output.</param>
        private static void FilterTypes(Assembly assembly, ClassTypeConstraintAttribute filter, List<Type> output)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsPublic || !type.IsClass) continue;
                if (filter != null && !filter.IsConstraintSatisfied(type)) continue;
                if (type.GetCustomAttributes(typeof(HideInInspector), true).FirstOrDefault() != null) continue;
                output.Add(type);
            }
        }

        /// <summary>
        /// Formats the name of the grouped type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="grouping">The grouping.</param>
        /// <returns>System.String.</returns>
        private static string FormatGroupedTypeName(Type type, ClassGrouping grouping)
        {
            var name = type.FullName;

            switch (grouping)
            {
                default:
                case ClassGrouping.None:
                    return name;

                case ClassGrouping.ByNamespace:
                    return name.Replace('.', '/');

                case ClassGrouping.ByNamespaceFlat:
                    var lastPeriodIndex = name.LastIndexOf('.');
                    if (lastPeriodIndex != -1) name = name.Substring(0, lastPeriodIndex) + "/" + name.Substring(lastPeriodIndex + 1);
                    return name;

                case ClassGrouping.ByAddComponentMenu:
                    var addComponentMenuAttributes = type.GetCustomAttributes(typeof(AddComponentMenu), false);
                    if (addComponentMenuAttributes.Length == 1) return ((AddComponentMenu)addComponentMenuAttributes[0]).componentMenu;
                    return "Scripts/" + type.FullName.Replace('.', '/');
            }
        }

        /// <summary>
        /// Gets the filtered types.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <returns>List&lt;Type&gt;.</returns>
        private static List<Type> GetFilteredTypes(ClassTypeConstraintAttribute filter)
        {
            var types = new List<Type>();

            var assembly = Assembly.GetExecutingAssembly();
            FilterTypes(assembly, filter, types);

            foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
            {
                FilterTypes(Assembly.Load(referencedAssembly), filter, types);
            }
            types.Sort((a, b) => a.FullName.CompareTo(b.FullName));

            return types;
        }

        /// <summary>
        /// Called when [selected type name].
        /// </summary>
        /// <param name="userData">The user data.</param>
        private static void OnSelectedTypeName(object userData)
        {
            var selectedType = userData as Type;
            s_SelectedClassRef = ClassTypeReference.GetClassRef(selectedType);
            var typeReferenceUpdatedEvent = EditorGUIUtility.CommandEvent("TypeReferenceUpdated");
            EditorWindow.focusedWindow.SendEvent(typeReferenceUpdatedEvent);
        }

        /// <summary>
        /// Resolves the type.
        /// </summary>
        /// <param name="classRef">The class reference.</param>
        /// <returns>Type.</returns>
        private static Type ResolveType(string classRef)
        {
            Type type;
            if (!s_TypeMap.TryGetValue(classRef, out type))
            {
                type = !string.IsNullOrEmpty(classRef) ? Type.GetType(classRef) : null;
                s_TypeMap[classRef] = type;
            }
            return type;
        }
        #endregion
    }
}
