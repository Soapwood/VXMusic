#if AIUNITY_CODE
namespace AiUnity.CLog.Core.Loggers
{
    public interface ISimpleLoggerSettings: ILoggerSettings
    {
        bool ShowCallsite { get; set; }
        bool ShowLogLevel { get; set; }
        bool ShowLoggerName { get; set; }
        bool ShowTimeStamp { get; set; }
        int StackIndex { get; set; }
        bool HeaderNewline { get; set; }
    }
}
#endif