// using NLog;
// using NLog.Config;
// using NLog.Targets;
//
// namespace Plugins.Logging
// {
//     public class Logger
//     {
//         public static void Configure()
//         {
//             var config = new LoggingConfiguration();
//             var logFile = new FileTarget("logfile")
//             {
//                 FileName = $"{UnityEngine.Application.persistentDataPath}/logs/logfile.txt"
//             };
//
//             config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);
//             LogManager.Configuration = config;
//         }
//     }
// }