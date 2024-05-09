// Copyright (C) 2014 Extesla, LLC.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    public class GameConsoleLogger : UnityConsoleLogger
    {
        #region Properties

        // Internal logger singleton
        private static IInternalLogger Logger { get { return CLogInternalLogger.Instance; } }

        /// <summary>
        /// Holds configuration settings associated with logger instance.
        /// </summary>
        public IGameConsoleLoggerSettings GameConsoleLoggerSettings { get; set; }

        /// <summary>
        /// Provides access to active Game Console controller script.
        /// </summary>
        private IGameConsoleController GameConsoleController { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates and initializes a logger that writes messages to Unity Console.
        /// </summary>
        /// <param name="logName">The name, usually type name of the calling class, of the logger.</param>
        /// <param name="context">GameObject associated with logger.</param>
        public GameConsoleLogger(string logName, IGameConsoleLoggerSettings gameConsoleLoggerSettings, UnityEngine.Object context, IFormatProvider formatProvider) : base(logName, gameConsoleLoggerSettings, context, formatProvider)
        {
            GameConsoleLoggerSettings = gameConsoleLoggerSettings;

            if (Application.isPlaying) {

                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.isLoaded) {
                    UpdateNLogMessageTarget(activeScene);
                }
                SceneManager.activeSceneChanged += (s1, s2) => UpdateNLogMessageTarget(s2);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Writes log message to the Unity Console.
        /// </summary>
        /// <param key="levels">The level of this log event.</param>
        /// <param key="message">The message to log</param>
        /// <param key="exception">The exception to log</param>
        protected override void WriteInternal(LogLevels levels, UnityEngine.Object context, object message, Exception exception)
        {
            string formattedMessage = FixUnityConsoleXML(FormatMessage(levels, message, exception));

            if (Application.isPlaying && GameConsoleController != null)
            //if (GameConsoleController != null)
            {
                GameConsoleController.AddMessage((int)levels, formattedMessage, Name, DateTime.Now);
            }
        }

        /// <summary>
        /// Initialize the Game Console GameController when active scene changes.
        /// </summary>
        /// <param key="scene">Active Unity scene.</param>
        private void UpdateNLogMessageTarget(Scene scene)
        {
            GameConsoleController = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<IGameConsoleController>()).FirstOrDefault();
            if (GameConsoleController != null) {
                GameConsoleController.SetIconEnable(GameConsoleLoggerSettings.IconEnable);
                GameConsoleController.SetConsoleActive(GameConsoleLoggerSettings.ConsoleActive);
                GameConsoleController.SetFontSize(GameConsoleLoggerSettings.FontSize);
                GameConsoleController.SetLogLevelFilter(GameConsoleLoggerSettings.LogLevelsFilter);
            }
            else {
                Logger.Error("Unable to locate GameConsole GameObject.  Please place NLog prefab GameConsole in your hierarchy.");
            }
        }
        #endregion
    }
}

#endif
