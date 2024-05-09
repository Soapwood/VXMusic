// ***********************************************************************
// Assembly         : Assembly-CSharp-Editor
// Author           : AiDesigner
// Created          : 07-09-2016
// Modified         : 02-02-2017
// ***********************************************************************
using System;
using UnityEditor;
using UnityEngine;

namespace AiUnity.Common.Editor.ModalWindow
{
    /// <summary>
    /// Define a popup modal window that return a result.
    /// Base class for IModal call implementation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="UnityEditor.EditorWindow" />
    public abstract class ModalWindow<T> : EditorWindow where T : ScriptableObject
    {
        #region Constants
        public const float TITLEBAR = 18;
        #endregion

        #region Fields
        private Action<WindowResult, T> handler;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public T Data { set; get; }

        /// <summary>
        /// Gets or sets the window result.
        /// </summary>
        public WindowResult WindowResult { get; protected set; }

        // protected properties...
        /// <summary>
        /// Gets the height.
        /// </summary>
        protected abstract float Height { get; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        protected abstract float Width { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Displays the specified handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="title">The title.</param>
        /// <param name="refPosition">The reference position.</param>
        public virtual void Display(Action<WindowResult, T> handler, string title, Rect refPosition)
        {
            this.titleContent = new GUIContent(title);
            this.handler = handler;

            var x = refPosition.x - (Width / 2);
            var y = refPosition.y;

            var rect = new Rect(x, y, 0, 0);
            this.position = rect;
            ShowAsDropDown(rect, new Vector2(Width, Height));
        }

        /// <summary>
        /// Cancels this instance.
        /// </summary>
        protected virtual void Cancel()
        {
            WindowResult = WindowResult.Cancel;
            if (this.handler != null) this.handler(WindowResult, Data);
            Close();
        }

        /// <summary>
        /// Draws the specified region.
        /// </summary>
        /// <param name="region">The region.</param>
        protected abstract void Draw(Rect region);

        /// <summary>
        /// Oks this instance.
        /// </summary>
        protected virtual void Ok()
        {
            WindowResult = WindowResult.Ok;
            if (this.handler != null) this.handler(WindowResult, Data);
            Close();
        }

        /// <summary>
        /// Called when [GUI].
        /// </summary>
        protected virtual void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(this.titleContent);
            GUILayout.EndHorizontal();
            var content = new Rect(0, TITLEBAR, position.width, position.height - TITLEBAR);
            GUILayout.BeginArea(content);
            GUILayout.Space(5);
            Draw(content);
            GUILayout.EndArea();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Called when [lost focus].
        /// </summary>
        protected virtual void OnLostFocus()
        {
            WindowResult = WindowResult.LostFocus;
            if (this.handler != null) this.handler(WindowResult, Data);
        }
        #endregion
    }
}
