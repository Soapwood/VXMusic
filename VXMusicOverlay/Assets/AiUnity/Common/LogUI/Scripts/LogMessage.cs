// ***********************************************************************
// Assembly         : Assembly-CSharp
// Author           : AiDesigner
// Created          : 06-20-2016
// Modified         : 05-06-2017
// ***********************************************************************
using AiUnity.Common.Log;
using System;

namespace AiUnity.Common.LogUI.Scripts
{
    /// <summary>
    /// Class LogMessage.
    /// </summary>
    public class LogMessage
    {
        #region Properties
        public string LoggerName { get; set; }
        public LogLevels LogLevels { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="LogMessage"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggerName">Name of the logger.</param>
        /// <param name="timeStamp">The time stamp.</param>
        public LogMessage(string message, int logLevel, string loggerName, DateTime timeStamp)
        {
            Message = message;
            LogLevels = (LogLevels)logLevel;
            LoggerName = loggerName;
            TimeStamp = timeStamp;
        }
        #endregion
    }
}