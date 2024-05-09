// ***********************************************************************
// Assembly         : Assembly-CSharp-Editor
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 02-02-2017
// ***********************************************************************
using AiUnity.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace AiUnity.Common.Editor.Drawers
{
    /// <summary>
    /// Custom drawer for read only properties.
    /// </summary>
    /// <seealso cref="UnityEditor.PropertyDrawer" />
    [CustomPropertyDrawer(typeof(UnityReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        #region Methods
        /// <summary>
        /// Override this method to specify how tall the GUI for this field is in pixels.
        /// </summary>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        /// <returns>The height in pixels.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        /// <summary>
        /// Grey out read only property by disabling GUI.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the property GUI.</param>
        /// <param name="property">The SerializedProperty to make the custom GUI for.</param>
        /// <param name="label">The label of this property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
        #endregion
    }
}
