// ***********************************************************************
// Assembly         : Assembly-CSharp-Editor
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 01-10-2017
// ***********************************************************************
using AiUnity.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace AiUnity.Common.Editor.Drawers
{
    /// <summary>
    /// Class DividerDrawer.
    /// </summary>
    /// <seealso cref="UnityEditor.DecoratorDrawer" />
    [CustomPropertyDrawer(typeof(DividerAttribute))]
public class DividerDrawer : DecoratorDrawer
{
    #region Fields
    public static Texture2D lineTex = null;
        #endregion


        #region Properties
        /// <summary>
        /// Gets the divider.
        /// </summary>
        private DividerAttribute divider
    {
        get { return ((DividerAttribute)attribute); }
    }
        #endregion

        #region Methods
        /// <summary>
        /// Override this method to specify how tall the GUI for this decorator is in pixels.
        /// </summary>
        /// <returns>System.Single.</returns>
        public override float GetHeight()
    {
        return base.GetHeight() + divider.space;
    }
        /// <summary>
        /// Override this method to make your own GUI for the decorator.
        /// See DecoratorDrawer for an example of how to use this.
        /// </summary>
        /// <param name="position">Rectangle on the screen to use for the decorator GUI.</param>
        public override void OnGUI(Rect position)
    {
        var co = Color.white;
        switch (divider.col.ToLower()) {
            case "white":
                co = Color.white;
                break;
            case "red":
                co = Color.red;
                break;
            case "blue":
                co = Color.blue;
                break;
            case "green":
                co = Color.green;
                break;
            case "gray":
                co = Color.gray;
                break;
            case "grey":
                co = Color.grey;
                break;
            case "black":
                co = Color.black;
                break;
        }

        lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, true);
        lineTex.SetPixel(0, 1, co);
        lineTex.Apply();
        var lineWidth = position.width * divider.widthPct;
        var lineX = ((position.x + position.width) - lineWidth - ((position.width - lineWidth) / 2));
        var lineY = position.y + (divider.space / 2) + 7.0f;
        var lineHeight = divider.thickness;
        EditorGUI.DrawPreviewTexture(new Rect(lineX, lineY, lineWidth, lineHeight), lineTex);
    }
    #endregion
}
}
