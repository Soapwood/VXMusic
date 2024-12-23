﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VXMusic.FileWriter;
using VXMusic.LogParser.Helpers;
using VXMusic.LogParser.Models;

namespace VXMusic.LogParser.VRChat
{
    public class VRChatLogParser : INotifyPropertyChanged
    {
        private readonly IServiceProvider _serviceProvider;
        private static ILogger<VRChatLogParser> _logger;
        
        public event PropertyChangedEventHandler? PropertyChanged;

        public static ConfigurationModel Configuration { get; set; }
        
        public static string LogFileName { get; set; }
        static readonly object logMutex = new object();

        public static Mutex ApplicationMutex; // TODO make priv
        public static bool HasApplicationMutex = false; // TODO make priv
        
        static volatile bool IsExiting = false;

        private static bool VrChatRuntimeLogDirExists => Directory.Exists(ConfigurationModel.VrChatRuntimeLogPath);

        static Task DispatchTask;

        static int DispatchResolutionMilliseconds = 50;

        static DateTime SilencedUntil = DateTime.Now;
        static DateTime LastMaximumKeywordsNotification = DateTime.Now;

        public static Dictionary<string, TailSubscription> Subscriptions { get; set; }

        static Timer LogDetectionTimer { get; set; }

        public static string LastKnownLocationName { get; set; } // World name
        public static string LastKnownLocationID { get; set; } // World ID
        static bool PlayerIsBetweenWorlds { get; set; }
        static bool NextJoinIsLocalUser { get; set; }
        
        public static ConcurrentQueue<string> MessageQueue = new ConcurrentQueue<string>();

        static ConcurrentQueue<NotificationDispatchModel> DispatchQueue = new ConcurrentQueue<NotificationDispatchModel>();

        private bool IsVRChatShuttingDown;
        private bool _isVrChatSessionRunning { get; set; }
        
        private string _currentVrChatWorld { get; set; }
        
        public bool IsVrChatSessionRunning
        {
            get { return _isVrChatSessionRunning; }
            set
            {
                _isVrChatSessionRunning = value;
                OnPropertyChanged(nameof(IsVrChatSessionRunning));
            }
        }
        
        public string CurrentVrChatWorld
        {
            get { return _currentVrChatWorld; }
            set
            {
                _currentVrChatWorld = value;
                OnPropertyChanged(nameof(CurrentVrChatWorld));
            }
        }

        public VRChatLogParser(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = _serviceProvider.GetService(typeof(ILogger<VRChatLogParser>)) 
                as ILogger<VRChatLogParser> ?? throw new ApplicationException("A logger must be created in service provider.");

            _logger.LogTrace("Creating VRChatLogParser.");

            Configuration = ConfigurationModel.Load();
            
            if (VrChatRuntimeLogDirExists)
            {
                Run();
            }
            else
            {
                _logger.LogWarning("VRChat Log directory not found. Will not Run VRChatLogParser.");
            }
        }

        public enum LogEventType
        {
            Error,
            Event,
            Info
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Run()
        {
            DateTime now = DateTime.Now;

            if (!Directory.Exists(ConfigurationModel.ExpandedUserFolderPath))
            {
                Directory.CreateDirectory(ConfigurationModel.ExpandedUserFolderPath);
                Directory.CreateDirectory($@"{ConfigurationModel.ExpandedUserFolderPath}\Logs");
            }

            LogFileName = $"Session_{now.Year:0000}{now.Month:00}{now.Day:00}{now.Hour:00}{now.Minute:00}{now.Second:00}.log";
            _logger.LogDebug($@"Log initialized at {ConfigurationModel.ExpandedUserFolderPath}\Logs\{LogFileName}");

            try
            {
                ApplicationMutex = new Mutex(true, "VXMVRCParser", out VRChatLogParser.HasApplicationMutex);

                ApplicationMutex.WaitOne(1000); // Wait around for a second.

                if (!HasApplicationMutex)
                {
                    _logger.LogError("Failed to obtain exclusivity. Is another parser instance running?");
                    Exit();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while attempting to determine exclusivity. Is another parser instance running?");
                _logger.LogError(ex.Message);
                Exit();
            }

            try
            {
                // Rewrite configuration to update it with any new fields not in existing configuration. Useful during update process and making sure the config always has updated annotations.
                // Users shouldn't need to re-configure every time they update the software.
                ConfigurationModel.Save(VRChatLogParser.Configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError("An exception occurred while attempting to read or write the configuration file.");
                _logger.LogError(ex.Message);
                Exit();
            }

            Subscriptions = new Dictionary<string, TailSubscription>();
            LastMaximumKeywordsNotification = now.AddSeconds(-Configuration.MaximumKeywordsExceededCooldownSeconds);
            LogDetectionTimer = new Timer(new TimerCallback(LogDetectionTick), null, 0, Configuration.DirectoryPollFrequencyMilliseconds);

            _logger.LogDebug($"Log detection timer initialized with poll frequency {Configuration.DirectoryPollFrequencyMilliseconds} and parse frequency {Configuration.ParseFrequencyMilliseconds}.");
        }

        void LogDetectionTick(object timerState)
        {
            string[] allFiles = Directory.GetFiles(Environment.ExpandEnvironmentVariables(Configuration.OutputLogRoot));
            foreach (string fn in allFiles)
                if (!Subscriptions.ContainsKey(fn) && fn.Contains("output_log"))
                {
                    try
                    {
                        Subscriptions.Add(fn,
                            new TailSubscription(fn, ParseTick, 0, Configuration.ParseFrequencyMilliseconds,
                                UnwindLogForMetadata));
                        _logger.LogTrace($"A tail subscription was added to {fn}");
                        
                    }
                    catch (ArgumentException ae)
                    {
                        _logger.LogWarning("Adding subscription failed. Subscription already exists. Client must be running.");
                    }
                }
        }

        static void RewindLogForMetadata(string filePath, long fromByte)
        {
            _logger.LogTrace($"Rewinding time to collect metadata for first time read of log at {filePath}...");

            string worldName = string.Empty;
            string instanceId = string.Empty;
            //int instanceCap = 0;

            byte[] firstNBytes = new byte[fromByte];

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                fs.Read(firstNBytes, 0, (int)fromByte);

            Span<byte> asByteSpan = new Span<byte>(firstNBytes);
            ReadOnlySpan<char> asCharSpan = Encoding.UTF8.GetString(asByteSpan).AsSpan();

            int lastIdx = asCharSpan.Length - 1;
            int currentIdx = asCharSpan.Length - 1;

            while (currentIdx > 0)
            {
                --currentIdx;

                if (asCharSpan[currentIdx] == '\n')
                {

                    string thisLine = asCharSpan.Slice(currentIdx, lastIdx - currentIdx).ToString();

                    if (thisLine.Contains("[Behaviour] Joining or"))
                    {
                        int tocLoc = 0;
                        string[] tokens = thisLine.Split(' ');
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == "Room:")
                            {
                                tocLoc = i;
                                break;
                            }
                        }

                        if (tokens.Length > tocLoc + 1)
                        {
                            string name = "";

                            for (int i = tocLoc + 1; i < tokens.Length; i++)
                                name += tokens[i] + " ";

                            worldName = name.Trim();
                        }
                    }
                    else if (thisLine.Contains("[Behaviour] Joining w"))
                    {
                        int tocLoc = 0;
                        string[] tokens = thisLine.Split(' ');
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            if (tokens[i] == "Joining")
                            {
                                tocLoc = i;
                                break;
                            }
                        }

                        if (tokens.Length > tocLoc + 1)
                            instanceId = tokens[tocLoc + 1];
                    }

                    lastIdx = currentIdx;
                }
            }

            if (worldName != null || instanceId != null)
            {
                // Read was presumably a success. Write values.
                LastKnownLocationName = worldName;
                LastKnownLocationID = instanceId;
                
                _logger.LogInformation($"Discovered instance {worldName} ({instanceId}).");
            }
            else
            {
                _logger.LogWarning($"No existing VRChat World instance found.");
            }
        }
        
        static void UnwindLogForMetadata(string filePath, long fromByte)
        {
            _logger.LogTrace($"Unwinding time to collect metadata for first time read of log at {filePath}...");

            string worldName = string.Empty;
            string instanceId = string.Empty;
            //int instanceCap = 0;

            byte[] firstNBytes = new byte[fromByte];

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                fs.Read(firstNBytes, 0, (int)fromByte);

            Span<byte> asByteSpan = new Span<byte>(firstNBytes);
            ReadOnlySpan<char> asCharSpan = Encoding.UTF8.GetString(asByteSpan).AsSpan();

            int lastIdx = 0;
            int currentIdx = 0;

            while (currentIdx < asCharSpan.Length)
            {
                if (asCharSpan[currentIdx] == '\n' || currentIdx == asCharSpan.Length - 1)
                {
                    // Adjust the slice to handle the line correctly
                    string thisLine = asCharSpan.Slice(lastIdx, currentIdx - lastIdx).ToString();

                    if (thisLine.Contains("[Behaviour] Joining or"))
                    {
                        string[] tokens = thisLine.Split(' ');
                        int tocLoc = Array.IndexOf(tokens, "Room:");
                        if (tocLoc != -1 && tokens.Length > tocLoc + 1)
                        {
                            worldName = string.Join(" ", tokens.Skip(tocLoc + 1)).Trim();
                        }
                    }
                    else if (thisLine.Contains("[Behaviour] Joining w"))
                    {
                        string[] tokens = thisLine.Split(' ');
                        int tocLoc = Array.IndexOf(tokens, "Joining");
                        if (tocLoc != -1 && tokens.Length > tocLoc + 1)
                        {
                            instanceId = tokens[tocLoc + 1];
                        }
                    }

                    lastIdx = currentIdx + 1; // Move past the newline for the next iteration
                }

                currentIdx++;
            }

            if (worldName != null || instanceId != null)
            {
                // Read was presumably a success. Write values.
                LastKnownLocationName = worldName;
                LastKnownLocationID = instanceId;
                
                _logger.LogInformation($"Discovered instance {worldName} ({instanceId}).");
            }
            else
            {
                _logger.LogWarning($"No existing VRChat World instance found.");
            }
        }

        /// <summary>
        /// This is messy, but they've changed format on me often enough that it's difficult to care!
        /// </summary>
        /// <param name="content"></param>
        void ParseTick(string content)
        {
            List<EventType> ToSend = new List<EventType>();

            if (!string.IsNullOrWhiteSpace(content))
            {
                if(!IsVrChatSessionRunning && !IsVRChatShuttingDown)
                    IsVrChatSessionRunning = true;

                if (!string.IsNullOrWhiteSpace(LastKnownLocationName))
                {
                    CurrentVrChatWorld = LastKnownLocationName;
                }
                
                string[] lines = content.Split('\n');

                foreach (string dirtyLine in lines)
                {
                    string line = Regex.Replace(dirtyLine
                        .Replace("\r", "")
                        .Replace("\n", "")
                        .Replace("\t", "")
                        .Trim(),
                        @"\s+", " ", RegexOptions.Multiline);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        int tocLoc = 0;
                        string[] tokens = line.Split(' ');

                        // Get new LastKnownLocationName here
                        if (line.Contains("Joining or"))
                        {
                            for (int i = 0; i < tokens.Length; i++)
                            {
                                if (tokens[i] == "Room:")
                                {
                                    tocLoc = i;
                                    break;
                                }
                            }

                            if (tokens.Length > tocLoc + 1)
                            {
                                string name = "";

                                for (int i = tocLoc + 1; i < tokens.Length; i++)
                                    name += tokens[i] + " ";

                                LastKnownLocationName = name.Trim();
                            }

                            PlayerIsBetweenWorlds = false;
                        }
                        // Get Current World/Room
                        else if (line.Contains("Successfully joined room"))
                        {
                            // IsVrChatSessionRunning = true;
                            // IsVRChatShuttingDown = false;
                            
                            SilencedUntil = DateTime.Now.AddSeconds(Configuration.WorldJoinSilenceSeconds);

                            ToSend.Add(EventType.WorldChange);

                            NextJoinIsLocalUser = true;

                            _logger.LogInformation($"World changed to {LastKnownLocationName} -> {LastKnownLocationID}");
                            CurrentVrChatWorld = LastKnownLocationName;
                            // TODO Wtf is happening here??
                            //_logger.LogInformation($"http://vrchat.com/home/launch?worldId={LastKnownLocationID.Replace(":", "&instanceId=")}", true);
                        }
                        // Instance left and/or client exited
                        else if (line.Contains("[Behaviour] OnLeftRoom"))
                        {
                            PlayerIsBetweenWorlds = true;
                            _logger.LogInformation($"Left world or exited client.");
                        }
                        else if (line.Contains("[Behaviour] Client invoked disconnect."))
                        {
                            IsVRChatShuttingDown = true;
                            _logger.LogWarning($"VRChat Client is disconnecting.");
                        }
                        else if (line.Contains("VRCApplication: OnApplicationQuit"))
                        {
                            IsVrChatSessionRunning = false;
                            _logger.LogWarning($"VRChat Client has shutdown.");
                        }
                        else if (line.Contains("[EOSManager] PlatformInterface.ShutDown"))
                        {
                            IsVRChatShuttingDown = false;
                            _logger.LogWarning($"EOS Manager has shutdown.");
                        }
                        // Get player leaves
                        else if (line.Contains("[Behaviour] OnPlayerLeft "))
                        {
                            for (int i = 0; i < tokens.Length; i++)
                            {
                                if (tokens[i] == "OnPlayerLeft")
                                {
                                    tocLoc = i;
                                    break;
                                }
                            }

                            string message = "";
                            string displayName = "";

                            if (tokens.Length > tocLoc + 1)
                            {
                                string name = "";

                                for (int i = tocLoc + 1; i < tokens.Length; i++)
                                    name += tokens[i] + " ";

                                displayName = name.Trim();

                                message += displayName;
                            }
                            else
                            {
                                message += "No username was provided.";
                            }

                            if (!PlayerIsBetweenWorlds)
                                ToSend.Add(EventType.PlayerLeft);
                        }
                    }
                }
            }

            if (ToSend.Count > 0)
            {
                foreach (EventType notification in ToSend)
                {
                    if (
                        (!CurrentlySilenced() && Configuration.DisplayPlayerJoined && notification == EventType.PlayerJoin)
                        || (!CurrentlySilenced() && Configuration.DisplayPlayerLeft && notification == EventType.PlayerLeft)
                        || (Configuration.DisplayWorldChanged && notification == EventType.WorldChange)
                        || (Configuration.DisplayPortalDropped && notification == EventType.PortalDropped)
                    )
                        DispatchQueue.Enqueue(new NotificationDispatchModel() { Type = notification });
                    else if (Configuration.DisplayMaximumKeywordsExceeded && notification == EventType.KeywordsExceeded
                        && DateTime.Now > LastMaximumKeywordsNotification.AddSeconds(Configuration.MaximumKeywordsExceededCooldownSeconds))
                    {
                        LastMaximumKeywordsNotification = DateTime.Now;
                        DispatchQueue.Enqueue(new NotificationDispatchModel() { Type = notification });
                    }
                }
            }
        }
        
        static bool CurrentlySilenced()
        {
            if (Configuration.DisplayJoinLeaveSilencedOverride)
                return false;

            if (DateTime.Now > SilencedUntil)
                return false;

            return true;
        }

        public static void Log(LogEventType type, string message, bool uiLogOnly = false)
        {
            DateTime now = DateTime.Now;
            string dateStamp = $"{now.Year:0000}/{now.Month:00}/{now.Day:00}";
            string timeStamp = $"{now.Hour:00}:{now.Minute:00}:{now.Second:00}";
            string typeStamp = $"{(type == LogEventType.Info ? "INFO" : (type == LogEventType.Event ? "EVENT" : "ERROR"))}";

            lock (logMutex)
            {
                EventLogAppend($"[{timeStamp}] <{typeStamp}> {message}\r\n");

                if (!uiLogOnly)
                    File.AppendAllText($@"{ConfigurationModel.ExpandedUserFolderPath}\Logs\{LogFileName}", $"[{dateStamp} {timeStamp}] [{typeStamp}] {message}\r\n");
            }
        }

        public static void Log(Exception ex)
        {
            Log(LogEventType.Error, $"{ex.Message}\r\n{ex.InnerException}\r\n{ex.StackTrace}");
        }

        public static void EventLogAppend(string message)
        {
            MessageQueue.Enqueue(message);
        }

        public static void Exit()
        {
            _logger.LogInformation("Cleaning up before termination.");

            IsExiting = true;

            if (DispatchTask != null)
                DispatchTask.Wait(DispatchResolutionMilliseconds * 2);

            if (Subscriptions != null)
            {
                foreach (var item in Subscriptions)
                    item.Value.Dispose();

                Subscriptions.Clear();

                _logger.LogInformation("Subscriptions cleared.");
            }

            if (HasApplicationMutex)
            {
                ApplicationMutex.ReleaseMutex();
                _logger.LogInformation("Application-level mutex released.");
            }

            _logger.LogInformation("Exiting.");

            Environment.Exit(-1);
        }
    }
}
