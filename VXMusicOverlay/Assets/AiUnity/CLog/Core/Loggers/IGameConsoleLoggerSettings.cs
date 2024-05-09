#if AIUNITY_CODE
using AiUnity.Common.Log;

namespace AiUnity.CLog.Core.Loggers
{
    public interface IGameConsoleLoggerSettings: IUnityConsoleLoggerSettings
    {
        bool ConsoleActive { get; set; }
        int FontSize { get; set; }
        bool IconEnable { get; set; }
        LogLevels LogLevelsFilter { get; set; }
    }
}
#endif