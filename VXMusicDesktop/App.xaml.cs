using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using NLog;

namespace VXMusicDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        public static VXMusicSession VXMusicSession;
        
        private static readonly string VxMusicLogo = """                                                                                          
                              --:                                                         
                           .=*#+                    .::                                   
               :=:       .+%%%=                     +#*       :--=.     #%:               
    -*.      -%@#:     -#@@%+@@%      .=*@@= *@*   -@@#  :+#@@@%*=:    %@@:  -+#@@@@@=    
    #@#    -%@@#@%-  =@@@%-.@@@@:  :*@@@@@%:%@@-   %@@==@@@*-.        #@@+.*@@#-.         
    %@%  -%@@@= .#@#@@@#- .@@@@@=+@@@@@@@@-@@@:   *@@@+@@@#%%@@@%*   #@@#=@@@-            
    @@# *@@@%.  .*@@@@:  .@@@+@@@@@@+*@@%.%@@:   +@@@=.---::::@@@@. #@@#-@@@=             
    %@%@@@@=   *@@@+#@*  %@%. *@@#= #@@* =@@#  :#@@@+      -*@@@%: #@@* *@@@-     .::.    
    +@@@@#.  .@@#-   -@@-%%    ..  *@@=  =@@@%@@@@*:   :=#@@@@#-  #@@+  =@@@@%#%%%%*-     
     =@#-     .        *@*         +*.    =#%##+-   :=*%@@%+-    +@@-    .=*****=-        
                        .*+        .             .:*#%*+-        +*.                      
                          :                       ..             .                        
""";
        // TODO https://blog.elmah.io/logging-and-global-error-handling-in-net-7-wpf-applications/
        public App()
        {
            _logger.Info(VxMusicLogo);
            _logger.Trace($"Booting VXMusic {DateTime.Now}");

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
    }
}
