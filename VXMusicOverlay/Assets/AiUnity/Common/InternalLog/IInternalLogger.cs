// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 12-01-2016
// Modified         : 07-23-2018
// ***********************************************************************
#if AIUNITY_CODE

using AiUnity.Common.Log;
using System;

namespace AiUnity.Common.InternalLog
{
    /// <summary>
    /// Interface  for <seealso cref="Common.InternalLogger" />
    /// </summary>
    public interface IInternalLogger
    {
        #region Properties
        /// <summary>
        /// Gets or sets the internal log level.
        /// </summary>
        LogLevels InternalLogLevel { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is assert enabled.
        /// </summary>
        bool IsAssertEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is debug enabled.
        /// </summary>
        bool IsDebugEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is error enabled.
        /// </summary>
        bool IsErrorEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is fatal enabled.
        /// </summary>
        bool IsFatalEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is information enabled.
        /// </summary>
        bool IsInfoEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is trace enabled.
        /// </summary>
        bool IsTraceEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is warn enabled.
        /// </summary>
        bool IsWarnEnabled { get; }
        #endregion

        #region Methods
        /// <summary>
        /// Logs the specified message at the Assert level.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Assert(string message, params object[] args);
        
        /// <summary>
        /// Logs the specified message at the Assert level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Assert(Exception e, string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Assert level.
        /// </summary>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Assert(bool condition, string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Debug level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Error(string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Error level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Error(Exception e, string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Fatal(string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Fatal level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Fatal(Exception e, string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Info level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Info(string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the specified level.
        /// </summary>
        /// <param name="levels">The levels.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Log(LogLevels levels, string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Trace level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Trace(string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Warn(string message, params object[] args);

        /// <summary>
        /// Logs the specified message at the Warn level.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments.</param>
        void Warn(Exception e, string message, params object[] args);

        #endregion
    }
}
#endif