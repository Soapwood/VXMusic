// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 05-06-2017
// ***********************************************************************
using UnityEngine;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class FocusPanel.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="UnityEngine.EventSystems.IPointerDownHandler" />
    public class FocusPanel : MonoBehaviour, IPointerDownHandler
    {
        #region Fields
        private RectTransform panel;
        #endregion

        #region Methods
        /// <summary>
        /// Called when [pointer down].
        /// </summary>
        /// <param name="data">The data.</param>
        public void OnPointerDown(PointerEventData data)
        {
            this.panel.SetAsLastSibling();
        }

        /// <summary>
        /// Awakes this instance.
        /// </summary>
        void Awake()
        {
            this.panel = GetComponent<RectTransform>();
        }
        #endregion
    }
}