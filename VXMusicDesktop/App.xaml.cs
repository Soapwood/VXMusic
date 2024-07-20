using System;
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
using System.Threading;
using VXMusic.VRChat;
using VXMusicDesktop.Core;
using VXMusicDesktop.Toast;
using VXMusicDesktop.Update;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

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
        
        public static Process? VXMOverlayProcess;
        
        // TODO Have you bumped the version of VXMusicDesktop?
        public App() : base()
        {
#if DEBUG
            ConsoleHelper.AllocConsole();
#endif
            this.Exit += new ExitEventHandler(VXMusicOverlay_Exit);
            
            ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            
            VXMusicSettingsSyncHandler.RunSettingsMigrationOnStartup();

            var overlaySettings = new OverlaySettings()
            {
                RuntimePath = configuration["Overlay:RuntimePath"],
                OverlayAnchor = VXUserSettings.Overlay.GetOverlayAnchor()
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
                    ClientId = configuration["Connections:Spotify:ClientId"],
                    PlaylistSavingSaveSetting = VXUserSettings.Connections.GetSpotifyPlaylistSaveSetting()
                },

                LastfmSettings = new LastfmSettings()
                {
                    ClientId = configuration["Connections:Lastfm:ClientId"],
                    AppToken = configuration["Connections:Lastfm:AppToken"],
                    Username = VXUserSettings.Connections.GetLastfmUsername(),
                    Password = VXUserSettings.Connections.GetLastfmPassword()
                }
            };

            VXMusicSession = new VXMusicSession(recognitionSettings, connectionsSettings, overlaySettings);

            ConfigureServices();
            
            Logger.LogInformation(ConsoleOutputBranding.VxMusicLogo + Environment.NewLine + ConsoleOutputBranding.CreatorInfo);
            Logger.LogTrace($"Booting VXMusic Desktop Client...");
            
            VXMusicSession.Initialise();

#if DEBUG
            // var _cancellationTokenSource = new CancellationTokenSource();
            // VXMusicOverlayInterface.StartVXMusicDesktopHeartbeatListener(_cancellationTokenSource.Token);
            // VXMusicOverlayInterface.StartVXMusicDesktopTcpServer(_cancellationTokenSource.Token);
            // VXMusicOverlayInterface.StartHeartbeatMonitoring();
#endif

            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Info, "Welcome to VXMusic!", "", 5);
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
            
            services.AddSingleton<VXMusicSettingsSyncHandler>();
            
            // Handle creating Recognition Clients based off of BYOAPI Configuration
            services.AddSingleton<ShazamClient>(serviceProvider => VXMusicSession.RecognitionSettings!.ShazamSettings.IsByoApiEnabled
                ? new ShazamClient(serviceProvider, VXMusicSession.RecognitionSettings.ShazamSettings.ByoApiKey!)
                : new ShazamClient(serviceProvider));
            
            services.AddSingleton<AudDClient>();

            services.AddSingleton<WindowsAudioDeviceListener>();
            services.AddSingleton<LastfmScrobbler>();
            services.AddSingleton<PlaylistFileWriter>();
            services.AddSingleton<XsOverlayNotificationClient>();
            services.AddSingleton<SteamVRNotificationClient>();
            services.AddSingleton<VrChatOscNotificationClient>();
            services.AddSingleton<SteamVROverlayAppsInterface>();
            
            services.AddSingleton<VXMusicUpdateHandler>();

            // services.AddSingleton<INotificationClient>(provider =>
            // {
            //     var selectedClient = new XsOverlayNotificationClient();
            //     var toastService = new ToastNotificationClient(w);
            //     return new ToastNotificationDecorator(selectedClient, toastService);
            // });
            
            services.AddSingleton<VRChatLogParser>();
            
            ServiceProvider = services.BuildServiceProvider();
            
            Logger = ServiceProvider.GetRequiredService<ILogger<App>>();
        }

        void VXMusicOverlay_Exit(object sender, ExitEventArgs e)
        {
            try
            {
                if (VXMOverlayProcess.Id >= 0)
                {
                    Process process = Process.GetProcessById(VXMOverlayProcess.Id);
                    process.Kill();
                    process.WaitForExit(); // Optional: Wait for the process to exit
                    Logger.LogDebug($"Successfully killed VXMusicOverlay process with PID {VXMOverlayProcess.Id}.");
                }
                else
                {
                    Logger.LogWarning("VXMusicOverlay Process was not found.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                Logger.LogDebug($"Error killing process: {ex.Message}");
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
