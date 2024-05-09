#if AIUNITY_CODE
namespace AiUnity.CLog.Core.Loggers
{
    public interface IUnityConsoleLoggerSettings : ISimpleLoggerSettings
    {
        bool EnableColors { get; set; }
        UnityEngine.Color HeaderColor { get; set; }
        UnityEngine.Color MessageColor { get; set; }
    }
}
#endif