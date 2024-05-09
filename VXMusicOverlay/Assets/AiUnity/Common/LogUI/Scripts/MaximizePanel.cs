// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 07-30-2016
// Modified         : 12-18-2017
// ***********************************************************************
using System;
using System.Linq;
using UnityEngine;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class MaximizePanel.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class MaximizePanel : MonoBehaviour
    {
        #region Fields
        //public GameObject GameConsolePanel;
        private RectTransform GameConsolePanelTransform;
        private bool isMaximized = false;
        private RectTransformStorage WindowStorage;
        #endregion

        #region Methods
        /// <summary>
        /// Maximums the handler.
        /// </summary>
        public void MaxHandler()
        {
            if (this.isMaximized)
            {
                this.WindowStorage.Restore(this.GameConsolePanelTransform);
                this.isMaximized = false;
            }
            else
            {
                this.WindowStorage.Store(this.GameConsolePanelTransform);
                this.GameConsolePanelTransform.anchorMin = new Vector2(0, 1);
                this.GameConsolePanelTransform.anchorMax = new Vector2(1, 1);
                //this.GameConsolePanelTransform.sizeDelta = Vector2.zero;
                this.GameConsolePanelTransform.sizeDelta = new Vector2(0f, this.GameConsolePanelTransform.sizeDelta.y);
                this.GameConsolePanelTransform.anchoredPosition = Vector3.zero;
                this.isMaximized = true;
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start()
        {
            this.GameConsolePanelTransform = (RectTransform)this.transform;
        }
        #endregion
    }
}