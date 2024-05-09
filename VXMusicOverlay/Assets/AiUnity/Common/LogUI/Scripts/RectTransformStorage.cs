// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 05-06-2017
// ***********************************************************************
using UnityEngine;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Struct RectTransformStorage
    /// </summary>
    public struct RectTransformStorage
    {
        #region Fields
        public Vector3 anchoredPosition;
        public Vector2 anchorMax;
        public Vector2 anchorMin;
        public Vector3 localScale;
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 sizeDelta;
        #endregion

        #region Methods
        /// <summary>
        /// Restores the specified t.
        /// </summary>
        /// <param name="t">The t.</param>
        public void Restore(RectTransform t)
        {
            t.anchorMin = this.anchorMin;
            t.anchorMax = this.anchorMax;
            t.sizeDelta = this.sizeDelta;

            t.position = this.position;
            t.rotation = this.rotation;
            t.localScale = this.localScale;
            t.anchoredPosition = this.anchoredPosition;
        }

        /// <summary>
        /// Stores the specified t.
        /// </summary>
        /// <param name="t">The t.</param>
        public void Store(RectTransform t)
        {
            this.anchorMin = t.anchorMin;
            this.anchorMax = t.anchorMax;
            this.sizeDelta = t.sizeDelta;

            this.position = t.position;
            this.rotation = t.rotation;
            this.localScale = t.localScale;
            this.anchoredPosition = t.anchoredPosition;
        }
        #endregion
    }
}