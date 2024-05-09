// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 11-03-2017
// ***********************************************************************
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class TogglePanelButton.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class ButtonStateManager : MonoBehaviour
    {
        #region Fields
        public bool pressed;
        public Sprite pressedSprite;

        private Image buttonImage;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the button.
        /// </summary>
        public Button Button { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// Toggles the panel.
        /// </summary>
        /// <param name="panel">The panel.</param>
        public void ToggleButtonState()
        {
            ToggleButtonState(!this.pressed);
        }

        /// <summary>
        /// Toggles the panel.
        /// </summary>
        /// <param name="panel">The panel.</param>
        public void ToggleButtonState(bool pressed)
        {
            this.pressed = pressed;
            buttonImage.overrideSprite = this.pressed ? pressedSprite : null;
            ButtonState buttonState = this.pressed ? ButtonState.Pressed : 0;
            ExecuteEvents.ExecuteHierarchy<IButtonStateChange>(gameObject, null, (x, y) => x.ButtonStateChange(buttonState));
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start()
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(ToggleButtonState);

            buttonImage = GetComponent<Image>();

            ToggleButtonState(true);
        }

        #endregion
    }
}