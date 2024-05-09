
// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

#if AIUNITY_CODE

namespace AiUnity.CLog.Core
{
    using System;
    using AiUnity.CLog.Core.Common;
    using AiUnity.Common.Extensions;
    using System.Diagnostics;
    using AiUnity.Common.Log;
    using AiUnity.CLog.Core.Loggers;

    /// <summary>
    /// Provides logging interface and utility functions.
    /// </summary>
    /// <tags>CLogAPI</tags>
    public abstract class CLogger : ILogger
    {
        /// <summary>
        /// The context if any associated with this logger.
        /// </summary>
        public UnityEngine.Object Context { get; protected set; }
        
        /// <summary>
        /// The name of this logger.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Holds configuration settings associated with logger instance.
        /// </summary>
        public ILoggerSettings LoggerSettings { get; private set; }

        /// <summary>
        /// The FormatProvider if any associated with this loggers.
        /// </summary>
        public IFormatProvider FormatProvider { get; protected set; }

        protected internal CLogger(string name, ILoggerSettings loggerSettings, UnityEngine.Object context = null, IFormatProvider formatProvider = null)
        {
            Name = name;
            LoggerSettings = loggerSettings;
            Context = context;
            FormatProvider = formatProvider;
        }

        #region Level Checks

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Trace</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Trace</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsTraceEnabled
        {
            get { return IsEnabled(LogLevels.Trace); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Debug</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Debug</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsDebugEnabled
        {
            get { return IsEnabled(LogLevels.Debug); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Info</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Info</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsInfoEnabled
        {
            get { return IsEnabled(LogLevels.Info); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Warn</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Warn</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsWarnEnabled
        {
            get { return IsEnabled(LogLevels.Warn); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Error</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Error</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsErrorEnabled
        {
            get { return IsEnabled(LogLevels.Error); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Fatal</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsFatalEnabled
        {
            get { return IsEnabled(LogLevels.Fatal); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the <c>Assert</c> level.
        /// </summary>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the <c>Fatal</c> level, otherwise it returns <see langword="false" />.</returns>
        public bool IsAssertEnabled
        {
            get { return IsEnabled(LogLevels.Assert); }
        }

        /// <summary>
        /// Gets a value indicating whether logging is enabled for the specified level.
        /// </summary>
        /// <param name="level">Log level to be checked.</param>
        /// <returns>A value of <see langword="true" /> if logging is enabled for the specified level, otherwise it returns <see langword="false" />.</returns>
        protected virtual bool IsEnabled(LogLevels level)
        {
            return LoggerSettings.LogLevels.Has(level);
        }

        #endregion

        #region Log() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, string message, params object[] args)
        {
            if (IsEnabled(level)) {
                WriteToTargets(level, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsEnabled(level)) {
                WriteToTargets(level, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, Exception exception, string message, params object[] args)
        {
            if (IsEnabled(level)) {
                WriteToTargets(level, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Log</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        public void Log(LogLevels level, UnityEngine.Object context, Exception exception, string message, params object[] args)
        {
            if (IsEnabled(level)) {
                WriteToTargets(level, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Trace() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(string message, params object[] args)
        {
            if (IsTraceEnabled) {
                WriteToTargets(LogLevels.Trace, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsTraceEnabled) {
                WriteToTargets(LogLevels.Trace, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(Exception exception, string message, params object[] args)
        {
            if (IsTraceEnabled) {
                WriteToTargets(LogLevels.Trace, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Trace</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_TRACE")]
        [Conditional("UNITY_EDITOR")]
        public void Trace(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsTraceEnabled) {
                WriteToTargets(LogLevels.Trace, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Debug() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(string message, params object[] args)
        {
            if (IsDebugEnabled) {
                WriteToTargets(LogLevels.Debug, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsDebugEnabled) {
                WriteToTargets(LogLevels.Debug, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(Exception exception, string message, params object[] args)
        {
            if (IsDebugEnabled) {
                WriteToTargets(LogLevels.Debug, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Debug</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_DEBUG")]
        [Conditional("UNITY_EDITOR")]
        public void Debug(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsDebugEnabled) {
                WriteToTargets(LogLevels.Debug, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Info() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(string message, params object[] args)
        {
            if (IsInfoEnabled) {
                WriteToTargets(LogLevels.Info, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsInfoEnabled) {
                WriteToTargets(LogLevels.Info, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(Exception exception, string message, params object[] args)
        {
            if (IsInfoEnabled) {
                WriteToTargets(LogLevels.Info, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Info</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_INFO")]
        [Conditional("UNITY_EDITOR")]
        public void Info(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsInfoEnabled) {
                WriteToTargets(LogLevels.Info, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Warn() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(string message, params object[] args)
        {
            if (IsWarnEnabled) {
                WriteToTargets(LogLevels.Warn, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsWarnEnabled) {
                WriteToTargets(LogLevels.Warn, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(Exception exception, string message, params object[] args)
        {
            if (IsWarnEnabled) {
                WriteToTargets(LogLevels.Warn, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Warn</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_WARN")]
        [Conditional("UNITY_EDITOR")]
        public void Warn(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsWarnEnabled) {
                WriteToTargets(LogLevels.Warn, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Error() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(string message, params object[] args)
        {
            if (IsErrorEnabled) {
                WriteToTargets(LogLevels.Error, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsErrorEnabled) {
                WriteToTargets(LogLevels.Error, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(Exception exception, string message, params object[] args)
        {
            if (IsErrorEnabled) {
                WriteToTargets(LogLevels.Error, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Error</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ERROR")]
        [Conditional("UNITY_EDITOR")]
        public void Error(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsErrorEnabled) {
                WriteToTargets(LogLevels.Error, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Fatal() overloads 
        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_FATAL")]
        [Conditional("UNITY_EDITOR")]
        public void Fatal(string message, params object[] args)
        {
            if (IsFatalEnabled) {
                WriteToTargets(LogLevels.Fatal, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_FATAL")]
        [Conditional("UNITY_EDITOR")]
        public void Fatal(UnityEngine.Object context, string message, params object[] args)
        {
            if (IsFatalEnabled) {
                WriteToTargets(LogLevels.Fatal, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_FATAL")]
        [Conditional("UNITY_EDITOR")]
        public void Fatal(Exception exception, string message, params object[] args)
        {
            if (IsFatalEnabled) {
                WriteToTargets(LogLevels.Fatal, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Fatal</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_FATAL")]
        [Conditional("UNITY_EDITOR")]
        public void Fatal(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsFatalEnabled) {
                WriteToTargets(LogLevels.Fatal, context, FormatProvider, message, args, exception);
            }
        }
        #endregion

        #region Assert() Overloads

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(bool test, string message, params object[] args)
        {
            if (IsAssertEnabled && !test) {
                WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(bool test, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsAssertEnabled && !test) {
                WriteToTargets(LogLevels.Assert, context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Exception exception, string message, params object[] args)
        {
            if (IsAssertEnabled) {
                WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="exception">An exception to be incorporated into log message.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Exception exception, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsAssertEnabled) {
                WriteToTargets(LogLevels.Assert, context, FormatProvider, message, args, exception);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Func<bool> test, string message, params object[] args)
        {
            if (IsAssertEnabled && !test()) {
                WriteToTargets(LogLevels.Assert, Context, FormatProvider, message, args);
            }
        }

        /// <summary>
        /// Logs a diagnostic message at the <c>Assert</c> level.
        /// </summary>
        /// <param name="test">Assert fires if test evaluates false.</param>
        /// <param name="context">Overrides the logger context for this message.  The context is the <see cref="GameObject"/>/<see cref="MonoBehaviour"/> associated with this log statement.</param>
        /// <param name="message">The message to be logged.</param>
        /// <param name="args">The arguments for a message containing <a href="https://msdn.microsoft.com/en-us/library/txafckwd(v=vs.110).aspx">composite formatting</a>.</param>
        [Conditional("CLOG_ALL")]
        [Conditional("CLOG_ASSERT")]
        [Conditional("UNITY_EDITOR")]
        public void Assert(Func<bool> test, UnityEngine.Object context, string message, params object[] args)
        {
            if (IsAssertEnabled && !test()) {
                WriteToTargets(LogLevels.Assert, context, FormatProvider, message, args);
            }
        }
        #endregion

        internal void WriteToTargets(LogLevels level, UnityEngine.Object context, IFormatProvider formatProvider, string message, object[] args, Exception exception = null)
        {
            if (args != null && args.Length > 0) {
                WriteInternal(level, context, new StringFormatFormattedMessage(formatProvider, message, args), exception);
            }
            else {
                WriteInternal(level, context, message, exception);
            }
        }

        /*protected virtual void WriteFilter(UnityEngine.Object context, LogLevels level, object message, Exception e = null)
        {
            if (LogManager.Instance.GlobalLogLevel.Has(level)) {
                WriteInternal(context, level, message, e);
            }
        }*/

        protected abstract void WriteInternal(LogLevels level, UnityEngine.Object context, object message, Exception e);

        /// <summary>
        /// Returns the global context for variables
        /// </summary>
        public virtual IVariablesContext GlobalVariablesContext
        {
            get { return new NoOpVariablesContext(); }
        }

        /// <summary>
        /// Returns the thread-specific context for variables
        /// </summary>
        /*public virtual IVariablesContext ThreadVariablesContext
        {
            get { return new Loggers.NoOpVariablesContext(); }
        }*/

    }
}
#endif
