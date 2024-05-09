// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 12-18-2017
// ***********************************************************************
using AiUnity.Common.Extensions;
using AiUnity.Common.Log;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if TMP_PRESENT
using TMPro;
#endif

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Controls GameConsole which is designed to log messages in the game.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    /// <seealso cref="AiUnity.Common.Log.IGameConsoleController" />
    public class GameConsoleController : MonoBehaviour, IGameConsoleController, IButtonStateChange
    {
        #region Fields
        public Toggle AutoScrollToggle;
        public Dropdown FilterDropDown;
        public InputField FontInputField;
        public GameObject GameConsoleIcon;
        public GameObject GameConsolePanel;
        public GameObject TextBoxPanel;
        public GameObject MoveButton;

#if TMP_PRESENT
        private TextMeshProUGUI messageText;
#else
        private Text messageText;
#endif

        private bool firstUpdate = true;
        private Image gameConsoleIconImage;
        private RectTransform gameConsoleIconTransform;
        private RectTransform gameConsolePanelTransform;
        private ScrollRect gameConsoleScrollRect;
        private bool iconEnable = true;
        private StringBuilder messageBuffer;
        private RectTransformStorage windowStorage;

        private GameObject titlePanel;
        #endregion

        #region Properties
        private LogLevels LogLevelsFilter { get; set; }
        private List<LogMessage> LogMessages { get; set; }
        private int MessageCharacters { get; set; }
        #endregion

        #region Methods
        void Awake()
        {
            LogMessages = new List<LogMessage>();
            this.messageBuffer = new StringBuilder();

            // Conditionally/Dynamically add EventSystem in case user already has it in the hierarchy
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
                eventSystem.AddComponent<StandaloneInputModule>();
            }

#if TMP_PRESENT
            //DestroyImmediate(TextBoxPanel.GetComponentInChildren<Text>());
            this.messageText = TextBoxPanel.GetComponentInChildren<TextMeshProUGUI>();

            if (this.messageText == null)
            {
                this.messageText = TextBoxPanel.AddComponent<TextMeshProUGUI>();
                TMP_FontAsset robotoFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/Roboto-Bold SDF");
                if (robotoFont != null)
                {
                    this.messageText.font = robotoFont;
                }

                //Debug.Log("offsetMin=" + rectTransform.offsetMin);
                //Debug.Log("offsetMax=" + rectTransform.offsetMax);
                //http://answers.unity3d.com/questions/859253/ui-how-to-configure-left-and-right.html
                RectTransform rectTransform = TextBoxPanel.GetComponent<RectTransform>();
                rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
                rectTransform.offsetMax = new Vector3(0, rectTransform.offsetMax.y);
            }
#else
            this.messageText = this.TextBoxPanel.GetComponentInChildren<Text>() ?? this.TextBoxPanel.AddComponent<Text>();
            this.messageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void Start()
        {
            this.FontInputField.onEndEdit.AddListener(i => SetFontSize(i, false));
            this.FilterDropDown.onValueChanged.AddListener(i => SetMaxLogLevelFilter(i, false));

            this.gameConsoleIconTransform = (RectTransform)this.GameConsoleIcon.transform;
            this.gameConsoleIconImage = this.GameConsoleIcon.GetComponent<Image>();
            this.gameConsolePanelTransform = (RectTransform)this.GameConsolePanel.transform;
            //this.moveButtonTransform = this.GameConsolePanel.GetComponentInChildren<ScrollRect>();
            this.gameConsoleScrollRect = this.GameConsolePanel.GetComponentsInChildren<ScrollRect>().FirstOrDefault();
            this.gameConsoleScrollRect.verticalNormalizedPosition = 0f;

            this.titlePanel = this.GameConsolePanel.transform.Find("TitlePanel").gameObject;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        void Update()
        {
            if (this.firstUpdate)
            {
                UpdateGameConsole();
                this.firstUpdate = false;
            }
            if (this.gameConsoleScrollRect != null)
            {
                if (this.AutoScrollToggle.isOn)
                {
                    this.gameConsoleScrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }

        public void ButtonStateChange(ButtonState buttonState)
        {
            //EnableDrag(!MoveButton.GetComponent<ButtonStateManager>().pressed);
            EnableDrag(buttonState == ButtonState.Pressed);
        }

        public void EnableDrag(bool enable)
        {
            foreach (DragPanel dragPanel in GetComponentsInChildren<DragPanel>(true))
            {
                dragPanel.enabled = enable;
            }
        }

        public void EnableConfigurationScreen()
        {
            //Image titleImage = this.GameConsolePanel.GetComponent<Image>();
            //titleImage.enabled = !titleImage.enabled;
            titlePanel.SetActive(!titlePanel.activeSelf);
        }

        /// <summary>
        /// Adds the message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="timeStamp">The time stamp.</param>
        public void AddMessage(int logLevel, string message, string loggerName = null, DateTime dateTime = default(DateTime))
        {
            LogMessage logMessage = new LogMessage(message, logLevel, loggerName, dateTime);
            LogMessages.Add(logMessage);

            if (((int)LogLevelsFilter & logLevel) == logLevel)
            {
                AppendMessage(message);
            }
        }

        /// <summary>
        /// Clears the messages.
        /// </summary>
        public void ClearMessages()
        {
            ReplaceBuffer(string.Empty);
        }

        /// <summary>
        /// Minimums the handler.
        /// </summary>
        /// <param name="minimize">if set to <c>true</c> [minimize].</param>
        /// <param name="animate">if set to <c>true</c> [animate].</param>
        /// <returns>IEnumerator.</returns>
        public IEnumerator MinHandler(bool minimize, bool animate = true)
        {
            if (minimize)
            {
                this.windowStorage.Store(this.gameConsolePanelTransform);
                this.GameConsoleIcon.SetActive(true);
                Vector3 targetPosition = this.gameConsoleIconTransform.position + (Vector3)this.gameConsoleIconTransform.rect.center;
                yield return StartCoroutine(AnimateGameConsole(targetPosition, Vector3.zero, 1f, animate));
                this.GameConsolePanel.SetActive(false);
            }
            else
            {
                this.GameConsolePanel.SetActive(true);
                yield return StartCoroutine(AnimateGameConsole(this.windowStorage.position, Vector3.one, 0, animate));
                this.GameConsoleIcon.SetActive(false);
            }
        }

        /// <summary>
        /// Minimums the handler animate.
        /// </summary>
        /// <param name="minimize">if set to <c>true</c> [minimize].</param>
        public void MinHandlerAnimate(bool minimize)
        {
            StartCoroutine(MinHandler(minimize));
        }

        /// <summary>
        /// Sets the console active.
        /// </summary>
        /// <param name="consoleActive">if set to <c>true</c> [console active].</param>
        public void SetConsoleActive(bool consoleActive)
        {
            this.GameConsolePanel.SetActive(consoleActive);
        }

        /// <summary>
        /// Sets the size of the font.
        /// </summary>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="updateControl">if set to <c>true</c> [update control].</param>
        /// <param name="updateMessage">if set to <c>true</c> [update message].</param>
        public void SetFontSize(int fontSize, bool updateControl = true, bool updateMessage = true)
        {
            if (updateControl)
            {
                this.FontInputField.text = fontSize.ToString();
            }
            if (updateMessage)
            {
                this.messageText.fontSize = fontSize;
            }
        }

        /// <summary>
        /// Sets the icon active.
        /// </summary>
        /// <param name="iconActive">if set to <c>true</c> [icon active].</param>
        public void SetIconActive(bool iconActive)
        {
            if (this.iconEnable)
            {
                this.GameConsoleIcon.SetActive(iconActive);
            }
        }

        /// <summary>
        /// Sets the icon enable.
        /// </summary>
        /// <param name="iconEnable">if set to <c>true</c> [icon enable].</param>
        public void SetIconEnable(bool iconEnable)
        {
            if (!iconEnable)
            {
                this.GameConsoleIcon.SetActive(false);
            }
            this.iconEnable = iconEnable;
        }

        /// <summary>
        /// Sets the log level filter.
        /// </summary>
        /// <param name="logLevels">The log levels.</param>
        /// <param name="updateControl">if set to <c>true</c> [update control].</param>
        /// <param name="updateMessage">if set to <c>true</c> [update message].</param>
        public void SetLogLevelFilter(LogLevels logLevels, bool updateControl = true, bool updateMessage = true)
        {
            LogLevelsFilter = logLevels;
            if (updateControl)
            {
                double maxLogLevel = (double)logLevels.GetFlags().Max();
                this.FilterDropDown.value = maxLogLevel > 0 ? (int)Math.Log(maxLogLevel, 2) + 1 : 0;
            }
            if (updateMessage)
            {
                FilterMessages();
            }
        }

        /// <summary>
        /// Sets the maximum log level filter.
        /// </summary>
        /// <param name="maxLevel">The maximum level.</param>
        /// <param name="updateControl">if set to <c>true</c> [update control].</param>
        /// <param name="updateMessage">if set to <c>true</c> [update message].</param>
        public void SetMaxLogLevelFilter(int maxLevel, bool updateControl = true, bool updateMessage = true)
        {
            SetLogLevelFilter((int)(Math.Pow(2, maxLevel) - 1), updateControl, updateMessage);
        }

        /// <summary>
        /// Animates the game console.
        /// </summary>
        /// <param name="targetConsolePosition">The target console position.</param>
        /// <param name="targetConsoleScale">The target console scale.</param>
        /// <param name="targetIconAlpha">The target icon alpha.</param>
        /// <param name="animate">if set to <c>true</c> [animate].</param>
        /// <returns>IEnumerator.</returns>
        IEnumerator AnimateGameConsole(Vector3 targetConsolePosition, Vector3 targetConsoleScale, float targetIconAlpha, bool animate)
        {
            float timeToLerp = 0.8f;
            float timeLerped = animate ? 0.0f : timeToLerp;
            Vector3 startingPosition = this.gameConsolePanelTransform.position;
            Vector3 endPosition = targetConsolePosition;
            Vector3 startingScale = this.gameConsolePanelTransform.localScale;

            float startAlpha = this.gameConsoleIconImage.color.a;
            Color sourceColor = this.gameConsoleIconImage.color;

            while (timeLerped <= timeToLerp)
            {
                timeLerped += Time.deltaTime;
                this.gameConsolePanelTransform.position = Vector3.Lerp(startingPosition, endPosition, timeLerped / timeToLerp);
                this.gameConsolePanelTransform.localScale = Vector3.Lerp(startingScale, targetConsoleScale, timeLerped / timeToLerp);
                sourceColor.a = Mathf.Lerp(startAlpha, targetIconAlpha, (timeLerped / timeToLerp) * (timeLerped / timeToLerp));
                this.gameConsoleIconImage.color = sourceColor;
                if (animate)
                {
                    yield return null;
                }
            }
        }

        private void AppendMessage(string message)
        {
            //Debug.Log("UpdateMessage: Message=" + message);
            //this.messageText.text += message + Environment.NewLine;
            this.messageText.text = this.messageBuffer.Append(message).ToString();
            LimitMessageLength();
        }

        /// <summary>
        /// Filters the messages.
        /// </summary>
        private void FilterMessages()
        {
            //AppendMessage(string.Join(Environment.NewLine, LogMessages.Where(m => LogLevelsFilter.Has(m.LogLevels)).Select(m => m.Message).ToArray()));
            //MessageBuffer = new StringBuilder(string.Join(Environment.NewLine, LogMessages.Where(m => LogLevelsFilter.Has(m.LogLevels)).Select(m => m.Message).ToArray()));
            //this.messageText.text = MessageBuffer.ToString();
            //LimitMessageLength();
            //ReplaceBuffer(string.Join(Environment.NewLine, LogMessages.Where(m => LogLevelsFilter.Has(m.LogLevels)).Select(m => m.Message).ToArray()));
            ReplaceBuffer(string.Concat(LogMessages.Where(m => LogLevelsFilter.Has(m.LogLevels)).Select(m => m.Message).ToArray()));
        }

#if TMP_PRESENT
        // Text Buffer 65K size limit is handled by TextMeshPro
        [Conditional("FALSE")]
#endif
        private void LimitMessageLength()
        {
            // Workaround for Unity3d text 65K limit - https://forum.unity3d.com/threads/ui-text-character-limit.359729
            if (this.messageText.text.Length > 12000)
            {
                int jumpForwardIndex = this.messageText.text.IndexOf(Environment.NewLine, 3000);
                this.messageBuffer = new StringBuilder(this.messageText.text.Substring(jumpForwardIndex));
                this.messageText.text = this.messageBuffer.ToString();
            }
        }

        private void ReplaceBuffer(string buffer)
        {
            this.messageBuffer = new StringBuilder(buffer);
            this.messageText.text = this.messageBuffer.ToString();
            LimitMessageLength();
        }

        /// <summary>
        /// Sets the size of the font.
        /// </summary>
        /// <param name="fontSizeText">The font size text.</param>
        /// <param name="updateControl">if set to <c>true</c> [update control].</param>
        /// <param name="updateMessage">if set to <c>true</c> [update message].</param>
        private void SetFontSize(string fontSizeText, bool updateControl = true, bool updateMessage = true)
        {
            int fontSize = 0;
            if (int.TryParse(fontSizeText, out fontSize))
            {
                SetFontSize(fontSize, updateControl, updateMessage);
            }
        }

        /// <summary>
        /// Sets the log level filter.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="updateControl">if set to <c>true</c> [update control].</param>
        /// <param name="updateMessage">if set to <c>true</c> [update message].</param>
        private void SetLogLevelFilter(int level, bool updateControl = true, bool updateMessage = true)
        {
            // Until Unity UI supports multi-selection enum fill in any missing one-hot bits.
            int maxLevel = level <= 0 ? level : (int)Math.Pow(2, (int)Math.Log(level, 2) + 1) - 1;
            SetLogLevelFilter(maxLevel.ToEnum<LogLevels>(), updateControl, updateMessage);
        }

        /// <summary>
        /// Updates the game console.
        /// </summary>
        void UpdateGameConsole()
        {
            this.windowStorage.Store(this.gameConsolePanelTransform);
            StartCoroutine(MinHandler(!this.GameConsolePanel.activeInHierarchy, false));
        }
        #endregion

    }
}