#if AIUNITY_CODE
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    public interface ILoggerSettings
    {
        /// <summary>
        /// The current logging threshold. Messages received that are beneath this threshold will not be logged.
        /// </summary>
        LogLevels LogLevels { get; set; }
    }
}
#endif