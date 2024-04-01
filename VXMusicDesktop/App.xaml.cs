﻿using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using VXMusic.Recognition.Shazam;
using VXMusic.Audio.Recording;
using VXMusic.Recognition.AudD;
using VXMusic.Branding;
using VXMusic.FileWriter;
using VXMusicDesktop.Console;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using VXMusic.Lastfm.Scrobbling;
using VXMusic.LogParser.VRChat;
using VXMusic.Overlay;
using VXMusic;
using System.Diagnostics;
using System.Reflection;
using VXMusicDesktop.MVVM.ViewModel;

namespace VXMusicDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ILogger Logger;
        public static ServiceProvider ServiceProvider;
        public static VXMusicSession VXMusicSession;
        public static Version ApplicationVersion;

        private static int VXMOverlayProcessId; 
        
        // TODO https://blog.elmah.io/logging-and-global-error-handling-in-net-7-wpf-applications/
        public App() : base()
        {
#if DEBUG
            ConsoleHelper.AllocConsole();
#endif
            //ConfigureServices();
            
            //Logger.LogInformation(ConsoleOutputBranding.VxMusicLogo + Environment.NewLine + ConsoleOutputBranding.CreatorInfo);
            //Logger.LogTrace($"Booting VXMusic Desktop Client...");
            
            ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version;

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var overlaySettings = new OverlaySettings()
            {
                RuntimePath = configuration["Overlay:RuntimePath"]
            };
            
            var recognitionSettings = new RecognitionSettings()
            {
                ShazamSettings = new ShazamSettings()
                {
                    ClientId = configuration["Recognition:Shazam:ClientId"],
                    ClientSecret = configuration["Recognition:Shazam:ClientSecret"],
                    X_RapidAPI_Key = configuration["Recognition:Shazam:X-RapidAPI-Key"],
                    X_RapidAPI_Host = configuration["Recognition:Shazam:X-RapidAPI-Host"],
                    IsByoApiEnabled = VXUserSettings.Recognition.GetIsShazamByoApiEnabled(),
                    ByoApiKey = VXUserSettings.Recognition.GetIsShazamByoApiEnabled() ? 
                        VXUserSettings.Recognition.GetShazamByoApiKey() : null
                },

                AudDSettings = new AudDSettings()
                {
                    ClientId = configuration["Recognition:Shazam:ClientId"],
                    IsByoApiEnabled = VXUserSettings.Recognition.GetIsAudDByoApiEnabled(),
                    ByoApiKey = VXUserSettings.Recognition.GetIsAudDByoApiEnabled() ? 
                        VXUserSettings.Recognition.GetAudDByoApiKey() : null
                }
            };

            var connectionsSettings = new ConnectionsSettings()
            {
                SpotifySettings = new SpotifySettings()
                {
                    ClientId = configuration["Connections:Spotify:ClientId"]
                },

                LastfmSettings = new LastfmSettings()
                {
                    ClientId = configuration["Connections:Lastfm:ClientId"],
                    ClientSecret = configuration["Connections:Lastfm:ClientSecret"],
                    Username = configuration["Connections:Lastfm:Username"],
                    Password = configuration["Connections:Lastfm:Password"]
                }
            };

            VXMusicSession = new VXMusicSession(recognitionSettings, connectionsSettings, overlaySettings);

            ConfigureServices();
            
            Logger.LogInformation(ConsoleOutputBranding.VxMusicLogo + Environment.NewLine + ConsoleOutputBranding.CreatorInfo);
            Logger.LogTrace($"Booting VXMusic Desktop Client...");
            
            VXMusicSession.Initialise();
            
            VXMusicOverlayInterface.StartVXMusicServerStream();
            
//#if RELEASE || DEBUG
            // if(overlaySettings.LaunchOverlayOnStartup)
            //     VXMOverlayProcessId = VXMusicOverlayInterface.LaunchVXMOverlayRuntime(overlaySettings.RuntimePath);
//#endif
        }

        public static void ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog("NLog.config"); // Configure NLog as needed
            });
            
            /*
             * Here we need to register all Clients that need to be used in the VXMusic Tool
             */
            services.AddSingleton<App>();
            
            // Handle creating Recognition Clients based off of BYOAPI Configuration
            services.AddSingleton<ShazamClient>(serviceProvider => VXMusicSession.RecognitionSettings!.ShazamSettings.IsByoApiEnabled
                ? new ShazamClient(serviceProvider, VXMusicSession.RecognitionSettings.ShazamSettings.ByoApiKey!)
                : new ShazamClient(serviceProvider));
            
            services.AddSingleton<AudDClient>();

            services.AddSingleton<WindowsAudioDeviceListener>();
            services.AddSingleton<LastfmScrobbler>();
            services.AddSingleton<PlaylistFileWriter>();
            services.AddSingleton<XSOverlay>();
            services.AddSingleton<SteamVRNotificationClient>();

            services.AddSingleton<VRChatLogParser>();

            //services.AddSingleton<ViewModelLocator>(serviceProvider => new ViewModelLocator(serviceProvider));
            
            ServiceProvider = services.BuildServiceProvider();
            
            Logger = ServiceProvider.GetRequiredService<ILogger<App>>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Your cleanup logic here
            base.OnExit(e);

            try
            {
                Process process = Process.GetProcessById(VXMOverlayProcessId);
                process.Kill();
                process.WaitForExit(); // Optional: Wait for the process to exit
                Logger.LogDebug($"Successfully killed process with PID {VXMOverlayProcessId}.");
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                Logger.LogDebug($"Error killing process: {ex.Message}");
            }
        }
    }
}
