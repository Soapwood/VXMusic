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

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class TogglePanelButton.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class TogglePanelButton : MonoBehaviour
    {
        #region Fields
        public bool panelOpen;
        public GameObject MenuPanel;
        //private Animator MenuPanelAnimator;
        private Sprite normalSprite;
        public Sprite pressedSprite;
        private LayoutElement MenuPanelLayoutElement;
        private float openHeight;

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
        public void TogglePanel()
        {
            TogglePanel(!this.panelOpen);
        }

        /// <summary>
        /// Toggles the panel.
        /// </summary>
        /// <param name="panel">The panel.</param>
        public void TogglePanel(bool panelOpen, bool animate = true)
        {
            this.panelOpen = panelOpen;
            //buttonImage.overrideSprite = this.panelOpen ? Button.spriteState.pressedSprite : null;
            buttonImage.overrideSprite = this.panelOpen ? pressedSprite : null;
            StartCoroutine(TogglePanelAnimate(animate));
        }

        public IEnumerator TogglePanelAnimate(bool animate = true)
        {

            float targetPosition = this.panelOpen ? openHeight : 0;
            MenuPanel.SetActive(true);
            yield return StartCoroutine(AnimateGameConsole(targetPosition, animate));
            MenuPanel.SetActive(this.panelOpen);
        }

        IEnumerator AnimateGameConsole(float targetHeight, bool animate)
        {
            float timeToLerp = 1f;
            float timeLerped = animate ? 0.0f : timeToLerp;
            float startHeight = MenuPanelLayoutElement.preferredHeight;

            while (timeLerped <= timeToLerp)
            {
                timeLerped += Time.deltaTime;
                MenuPanelLayoutElement.preferredHeight = Mathf.Lerp(startHeight, targetHeight, timeLerped / timeToLerp);

                if (animate)
                {
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start()
        {
            MenuPanelLayoutElement = this.MenuPanel.GetComponent<LayoutElement>();
            openHeight = MenuPanelLayoutElement.preferredHeight;

            Button = GetComponent<Button>();
            Button.onClick.AddListener(TogglePanel);

            buttonImage = GetComponent<Image>();
            //buttonImage.sprite;

            this.panelOpen = true;
            TogglePanel(this.panelOpen, false);
        }

        #endregion
    }
}