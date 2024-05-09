// ***********************************************************************
// Assembly   : Assembly-CSharp
// Company    : AiUnity
// Author     : AiDesigner
//
// Created    : 12-01-2016
// Modified   : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using UnityEngine;

namespace AiUnity.Common.InternalLog
{
    using AiUnity.Common.Extensions;
    using AiUnity.Common.Log;
    using AiUnity.Common.Patterns;
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Internal Unity logger that can be customized in derived class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="AiUnity.Common.Patterns.Singleton{T}" />
    /// <seealso cref="AiUnity.Common.InternalLog.IInternalLogger" />
    //public abstract class InternalLogger<T> : Singleton<T>, IInternalLogger where T : InternalLogger<T>
    public abstract class InternalLogger<T> : Singleton<T>, IInternalLogger where T : new()
    {
        #region Fields
        /// <summary> The internal log level </summary>
        private LogLevels internalLogLevel;
        /// <summary> The is dark theme </summary>
        private bool isDarkTheme = false;

        /// <summary> The log level player preference </summary>
        private string logLevelPlayerPref;
        #endregion

        #region Properties
        /// <summary> Gets or sets the internal log level. </summary>
        public LogLevels InternalLogLevel {
            get { return this.internalLogLevel; }
            set {
                if (this.internalLogLevel != value)
                {
                    Logger.Info("Setting {0} (PlayerPref=\"{1}\") to LogLevel={2}", GetType().Name, this.logLevelPlayerPref, value);
                    PlayerPrefs.SetInt(this.logLevelPlayerPref, (int)value);
                    this.internalLogLevel = value;

                    if (Logger != this)
                    {
                        Logger.InternalLogLevel = value;
                    }
                }
            }
        }

        /// <summary> Gets a value indicating whether logging is enabled for the <c>Assert</c> level. </summary>
        public bool IsAssertEnabled {
            get { return IsEnabled(LogLevels.Assert); }
        }

        /// <summary> Gets a value indicating whether logging is enabled for the <c>Debug</c> level. </summary>
        public bool IsDebugEnabled {
            get { return IsEnabled(LogLevels.Debug); }
        }

        /// <summary> Gets a value indicating whether logging is enabled for the <c>Error</c> level. </summary>
        public bool IsErrorEnabled {
            get { return IsEnabled(LogLevels.Error); }
        }

        /// <summary> Gets a value indicating whether logging is enabled for the <c>Fatal</c> level. </summary>
        public bool IsFatalEnabled {
            get { return IsEnabled(LogLevels.Fatal); }
        }

        /// <summary> Gets a value indicating whether logging is enabled for the <c>Info</c> level. </summary>
        public bool IsInfoEnabled {
            get { return IsEnabled(LogLevels.Info); }
        }

        /// <summary> Gets a value indicating whether logging is enabled for the <c>Trace</c> level. </summary>
        public bool IsTraceEnabled {
            get { return IsEnabled(LogLevels.Trace); }
        }

        /// <summary> Gets a value indicating whether logging is enabled for the <c>Warn</c> level. </summary>
        public bool IsWarnEnabled {
            get { return IsEnabled(LogLevels.Warn); }
        }

        /// <summary> Gets or sets a value indicating whether timestamp should be included in internal log output. </summary>
        protected bool IncludeTimestamp { get; set; }

        /// <summary> Gets or sets the Default log level should a PlayerPref key not be present. </summary>
        protected LogLevels InternalLogLevelsDefault { get; set; }

        /// <summary> Gets the logger. </summary>
        private static IInternalLogger Logger { get { return CommonInternalLogger.Instance; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InternalLogger{T}" /> class.
        /// </summary>
        public InternalLogger()
        {
            this.logLevelPlayerPref = "AiUnity" + GetType().Name + "LogLevel";
            this.internalLogLevel = PlayerPrefs.GetInt(this.logLevelPlayerPref, (int)InternalLogLevelsDefault).ToEnum<LogLevels>();
            this.isDarkTheme = Application.isEditor && Convert.ToBoolean(PlayerPrefs.GetInt("AiUnityIsProSkin", 0));
            InternalLogLevelsDefault = LogLevels.Fatal | LogLevels.Error | LogLevels.Warn | LogLevels.Info;
            IncludeTimestamp = false;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Logs the specified message at the Assert level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Assert(string message, params object[] args)
        {
            if (IsAssertEnabled)
            {
                Write(LogLevels.Assert, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Assert level.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Assert(bool condition, string message, params object[] args)
        {
            if (IsAssertEnabled && !condition)
            {
                Write(LogLevels.Assert, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Assert level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Assert(Exception e, string message, params object[] args)
        {
            if (IsAssertEnabled && e != null)
            {
                string exceptionMessage = string.Format("{0}/n<color=#ff0000ff>{1}</color>", message, e != null ? e.Message : "Missing exception message");
                Write(LogLevels.Assert, exceptionMessage, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Debug(string message, params object[] args)
        {
            if (IsDebugEnabled)
            {
                Write(LogLevels.Debug, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Error(string message, params object[] args)
        {
            if (IsErrorEnabled)
            {
                Write(LogLevels.Error, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Error(Exception e, string message, params object[] args)
        {
            if (IsErrorEnabled)
            {
                string exceptionMessage = e != null ? e.Message : "Missing exception message";
                string combinedMessage = string.Format("{0}/n<color=#ff0000ff>{1}</color>", message, exceptionMessage);
                Write(LogLevels.Error, combinedMessage, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Fatal(string message, params object[] args)
        {
            if (IsFatalEnabled)
            {
                Write(LogLevels.Fatal, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Fatal(Exception e, string message, params object[] args)
        {
            if (IsFatalEnabled)
            {
                string exceptionMessage = e != null ? e.Message : "Missing exception message";
                string combinedMessage = string.Format("{0}/n<color=#ff0000ff>{1}</color>", message, exceptionMessage);
                Write(LogLevels.Fatal, combinedMessage, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Info(string message, params object[] args)
        {
            if (IsInfoEnabled)
            {
                Write(LogLevels.Info, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the specified level.
        /// </summary>
        /// <param name="levels">Log level.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Log(LogLevels levels, string message, params object[] args)
        {
            if (IsEnabled(levels))
            {
                Write(levels, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Trace(string message, params object[] args)
        {
            if (IsTraceEnabled)
            {
                Write(LogLevels.Trace, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Warn(string message, params object[] args)
        {
            if (IsWarnEnabled)
            {
                Write(LogLevels.Warn, message, args);
            }
        }

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">Message which may include positional parameters.</param>
        /// <param name="args">Arguments to the message.</param>
        public void Warn(Exception e, string message, params object[] args)
        {
            if (IsErrorEnabled)
            {
                string exceptionMessage = e != null ? e.Message : "Missing exception message";
                string combinedMessage = string.Format("{0}/n<color=#ff0000ff>{1}</color>", message, exceptionMessage);
                Write(LogLevels.Warn, combinedMessage, args);
            }
        }

        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="levels">The levels.</param>
        /// <param name="formattedMessage">The formatted message.</param>
        /// <returns>Message to be logged.</returns>
        protected virtual string CreateMessage(LogLevels levels, string formattedMessage)
        {
            string callerName;

            try
            {
                StackTrace stackTrace = new StackTrace(true);
                StackFrame stackFrame = stackTrace.GetFrames().SkipWhile(f => typeof(IInternalLogger).IsAssignableFrom(f.GetMethod().DeclaringType)).FirstOrDefault();
                callerName = string.Format("{0}.{1}", stackFrame.GetMethod().DeclaringType.FullName, stackFrame.GetMethod().Name);
            }
            catch
            {
                callerName = string.Format("AiUnity Internal Message ({0})", GetType().Name);
            }

            string header = string.Format("[{0} (ScriptBuilder Internal)] {1}", levels, callerName);

            if (Application.isEditor)
            {
                // Good Dark theme colors: yellow, cyan, orange, lime, red, olive, white
                // Good Light theme colors: teal, blue, magenta, black
                // Decent both theme colors: black, red, olive
                string headerColor = this.isDarkTheme ? "orange" : "magenta";
                header = string.Format("<color={0}>{1}</color>", headerColor, header);
            }
            return header + Environment.NewLine + formattedMessage;
        }

        /// <summary>
        /// Performs the underlining write to the Unity Console
        /// </summary>
        /// <param name="levels">The levels.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        //protected abstract void Write(LogLevels levels, string message, object[] args);
        protected void Write(LogLevels levels, string message, object[] args)
        {
            string formattedMessage = (args != null && args.Length > 0) ? string.Format(CultureInfo.InvariantCulture, message, args) : message;
            string internalMessage = CreateMessage(levels, formattedMessage);

            if (levels.Has(LogLevels.Info) || levels.Has(LogLevels.Debug) || levels.Has(LogLevels.Trace))
            {
                UnityEngine.Debug.Log(internalMessage);
            }
            else if (levels.Has(LogLevels.Warn))
            {
                UnityEngine.Debug.LogWarning(internalMessage);
            }
            else if (levels.Has(LogLevels.Error) || levels.Has(LogLevels.Fatal))
            {
                UnityEngine.Debug.LogError(internalMessage);
            }
            else if (levels.Has(LogLevels.Assert))
            {
                UnityEngine.Assertions.Assert.IsTrue(false, internalMessage);
            }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        private bool IsEnabled(LogLevels level)
        {
            return InternalLogLevel.Has(level);
        }
        #endregion
    }
}

#endif