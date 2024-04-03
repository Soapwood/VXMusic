using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic;
using VXMusicDesktop.Theme;

namespace VXMusicDesktop;

public class VXUserSettings
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();

    internal class Notifications
    {
        public static void SetNotificationServiceInSettings(NotificationService api)
        {
            Properties.Settings.Default.NotificationService = api.ToString();
            Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting NotificationService set to {Properties.Settings.Default.NotificationService}");
        }
        
        public static NotificationService GetCurrentNotificationService()
        {
            Enum.TryParse<NotificationService>(Properties.Settings.Default.NotificationService, out var currentNotificationService);
            return currentNotificationService;
        }
    }

    internal class Recognition
    {
        public static RecognitionApi GetCurrentRecognitionApi()
        {
            Enum.TryParse<RecognitionApi>(Properties.Settings.Default.RecognitionApi, out var currentRecognitionApi);
            return currentRecognitionApi;
        }

        public static String GetRecognitionApiString()
        {
            return Properties.Settings.Default.RecognitionApi;
        }

        public static void SetRecognitionApi(RecognitionApi api)
        {
            Properties.Settings.Default.RecognitionApi = api.ToString();
            Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting RecognitionApi set to {Properties.Settings.Default.RecognitionApi}");
        }

        public static void SetIsShazamByoApiEnabled(bool isByoApiEnabled)
        {
            Properties.Settings.Default.IsShazamByoApiEnabled = isByoApiEnabled;
            Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting IsShazamByoApiEnabled set to {Properties.Settings.Default.IsShazamByoApiEnabled.ToString()}");
        }

        public static bool GetIsShazamByoApiEnabled()
        {
            return Properties.Settings.Default.IsShazamByoApiEnabled;
        }

        public static void SetShazamByoApiKey(string byoApiKey)
        {
            Properties.Settings.Default.ShazamByoApiKey = byoApiKey;
            Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting ShazamByoApiKey set to {Properties.Settings.Default.ShazamByoApiKey}");
        }

        public static string GetShazamByoApiKey()
        {
            return Properties.Settings.Default.ShazamByoApiKey;
        }

        public static void SetIsAudDByoApiEnabled(bool isByoApiEnabled)
        {
            Properties.Settings.Default.IsAudDByoApiEnabled = isByoApiEnabled;
            Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting IsAudDByoApiEnabled set to {Properties.Settings.Default.IsShazamByoApiEnabled}");
        }

        public static bool GetIsAudDByoApiEnabled()
        {
            return Properties.Settings.Default.IsAudDByoApiEnabled;
        }

        public static void SetAudDByoApiKey(string byoApiKey)
        {
            Properties.Settings.Default.AudDByoApiKey = byoApiKey;
            Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting AudDByoApiKey set to {Properties.Settings.Default.AudDByoApiKey}");
        }

        public static string GetAudDByoApiKey()
        {
            return Properties.Settings.Default.AudDByoApiKey;
        }
    }
    
    internal class Connections
    {
        public static string GetLastfmUsername()
        {
            return VXMusicDesktop.Properties.Settings.Default.LastfmUsername;
        }
        
        public static void SetLastfmUsername(string username)
        {
            VXMusicDesktop.Properties.Settings.Default.LastfmUsername = username;
        }
        
        public static string GetLastfmPassword()
        {
            return VXMusicDesktop.Properties.Settings.Default.LastfmPassword;
        }
        
        public static void SetLastfmPassword(string password)
        {
            VXMusicDesktop.Properties.Settings.Default.LastfmPassword = password;
        }
    }

    internal class Overlay
    {
        public static void SetLaunchOverlayOnStartup(bool launchOverlayOnStartup)
        {
            VXMusicDesktop.Properties.Settings.Default.LaunchOverlayOnStartup = launchOverlayOnStartup;
            VXMusicDesktop.Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting LaunchOverlayOnStartup set to {Properties.Settings.Default.LaunchOverlayOnStartup}");
        }
    
        public static bool GetCurrentOverlayLaunchOnStartup()
        {
            return VXMusicDesktop.Properties.Settings.Default.LaunchOverlayOnStartup;
        }
    }

    internal class Desktop
    {
        public static DesktopTheme GetCurrentDesktopTheme()
        {
            Enum.TryParse<DesktopTheme>(VXMusicDesktop.Properties.Settings.Default.DesktopTheme, out DesktopTheme desktopTheme);
            return desktopTheme;
        }
        
        public static void SetDesktopThemeInSettings(DesktopTheme theme)
        {
            VXMusicDesktop.Properties.Settings.Default.DesktopTheme = theme.ToString();
            VXMusicDesktop.Properties.Settings.Default.Save();
            
            Logger.LogDebug($"User Setting DesktopTheme set to {Properties.Settings.Default.DesktopTheme}");
        }
    }
}