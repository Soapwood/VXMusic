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
        
        // TODO https://blog.elmah.io/logging-and-global-error-handling-in-net-7-wpf-applications/
        public App()
        {
#if DEBUG
            ConsoleHelper.AllocConsole();
#endif
            
            ConfigureLogging();
            
            Logger.LogInformation(ConsoleOutputBranding.VxMusicLogo + Environment.NewLine + ConsoleOutputBranding.CreatorInfo);
            Logger.LogTrace($"Booting VXMusic...");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var recognitionSettings = new RecognitionSettings()
            {
                ShazamSettings = new ShazamSettings()
                {
                    ClientId = configuration["Recognition:Shazam:ClientId"],
                    ClientSecret = configuration["Recognition:Shazam:ClientSecret"],
                    X_RapidAPI_Key = configuration["Recognition:Shazam:X-RapidAPI-Key"],
                    X_RapidAPI_Host = configuration["Recognition:Shazam:X-RapidAPI-Host"]
                },

                AudDSettings = new AudDSettings()
                {
                    ClientId = configuration["Recognition:Shazam:ClientId"],
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

            VXMusicSession = new VXMusicSession(recognitionSettings, connectionsSettings);
        }

        public static void ConfigureLogging()
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
                
            services.AddTransient<VRChatLogParser>();
            services.AddTransient<ShazamClient>();
            services.AddTransient<AudDClient>();
            services.AddTransient<WindowsAudioDeviceListener>();
            services.AddTransient<LastfmScrobbler>();
            services.AddTransient<PlaylistFileWriter>();
            
            ServiceProvider = services.BuildServiceProvider();
            
            Logger = ServiceProvider.GetRequiredService<ILogger<App>>();
        }
    }
}
