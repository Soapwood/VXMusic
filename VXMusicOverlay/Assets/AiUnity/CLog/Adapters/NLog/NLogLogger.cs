#if AIUNITY_CODE

using System;
using AiUnity.CLog.Core;
using AiUnity.Common.InternalLog;
using AiUnity.Common.Log;
using AiUnity.CLog.Core.Loggers;

namespace AiUnity.CLog.Adapters.NLog
{
    /// <summary>
    /// Logger interface for Unity NLog Asset
    /// </summary>
    public class NLogLogger : CLogger
    {

        #region Fields

        private readonly ILogger logger;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        protected internal NLogLogger(string name, ILoggerSettings loggerSettings, ILogger logger, UnityEngine.Object context) : base(name, loggerSettings, context)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Actually sends the message to the underlying log system.
        /// </summary>
        /// <param name="logLevel">the level of this log event.</param>
        /// <param name="message">the message to log</param>
        /// <param name="exception">the exception to log (may be null)</param>
        protected override void WriteInternal(LogLevels logLevel, UnityEngine.Object context, object message, Exception exception)
        {
            logger.Log(logLevel, context, exception, message.ToString());
        }

    }
}

#endif
