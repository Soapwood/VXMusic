
#region License

/*
 * Copyright � 2002-2009 the original author or authors.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core.Common;
using System.Globalization;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using AiUnity.CLog.Core.Configuration;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    /// <summary>
    /// Abstract class providing a standard implementation of simple loggers.
    /// </summary>
    [Serializable]
    public abstract class SimpleLogger : CLogger
    {
        #region Properties
        /// <summary>
        /// Holds configuration settings associated with logger instance.
        /// </summary>
        public ISimpleLoggerSettings SimpleLoggerSettings { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates and initializes a the simple logger.
        /// </summary>
        /// <param name="logName">The key, usually type key of the calling class, of the logger.</param>
        public SimpleLogger(string logName, ISimpleLoggerSettings simpleLoggerSettings, UnityEngine.Object context, IFormatProvider formatProvider) : base(logName, simpleLoggerSettings, context, formatProvider)
        {
            SimpleLoggerSettings = simpleLoggerSettings;
        }
        #endregion

        #region Methods
        protected virtual void CreateExceptionMessage(StringBuilder stringBuilder, Exception e)
        {
            if (e != null) {
                ExceptionBuilder exceptionBuilder = new ExceptionBuilder(stringBuilder);
                exceptionBuilder.Format(e);
            }
        }

        /// <summary>
        /// Appends the formatted message to the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="stringBuilder">the <see cref="StringBuilder"/> that receives the formatted message.</param>
        /// <param name="levels"></param>
        /// <param name="message"></param>
        /// <param name="e"></param>
        protected virtual string CreateMessageInfo(LogLevels levels, int stackOffset)
        {
            StringBuilder stringBuilder = new StringBuilder();

            // Append a readable representation of the log level
            if (SimpleLoggerSettings.ShowLogLevel) {
                stringBuilder.Append((string.Format("[{0}]", levels.ToString().ToUpper())).PadRight(8));
            }

            // Append the key of the log instance if so configured
            if (SimpleLoggerSettings.ShowLoggerName) {
                stringBuilder.Append(Name + ": ");
            }

            if (SimpleLoggerSettings.ShowCallsite) {
                try {
                    StackTrace stackTrace = new StackTrace();
                    int stackPosition = Math.Min(Math.Max(stackOffset + SimpleLoggerSettings.StackIndex, 0), stackTrace.FrameCount - 1);
                    MethodBase methodBase = stackTrace.GetFrame(stackPosition).GetMethod();
                    stringBuilder.AppendFormat("{0}.{1} ", methodBase.DeclaringType, methodBase.Name);
                }
                catch {
                    stringBuilder.Append("Unknown caller type/method ");
                }
            }

            // Append date-time if so configured
            if (SimpleLoggerSettings.ShowTimeStamp) {
                stringBuilder.AppendFormat("({0}) ", DateTime.Now);
            }

            /*if (stringBuilder.Length > 0)
            {
                stringBuilder.Length--;

                if (SimpleLoggerSettings.HeaderNewline)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
                else
                {
                    stringBuilder.Append(":: ");
                }
            }*/

            return stringBuilder.ToString();
        }
        #endregion
    }

}
#endif
