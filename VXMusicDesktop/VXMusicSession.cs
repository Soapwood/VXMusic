using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using VXMusic;
using VXMusic.API;
using VXMusic.Audio;
using VXMusic.Audio.Recording;
using VXMusic.FileWriter;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;
using VXMusic.Lastfm.Scrobbling;
using VXMusic.LogParser.VRChat;
using VXMusic.Overlay;
using System.Windows.Media;
using VXMusicDesktop.Theme;

//using System.Windows.Forms.PropertyGridInternal;

namespace VXMusicDesktop;

public class VXMusicSession
{
    // Current Recognition Options
    // Spotify Connected
    // Notification Options
    
    public RecognitionSettings? RecognitionSettings;
    public static NotificationSettings? NotificationSettings;
    public static ConnectionsSettings? ConnectionsSettings;
    public static DesktopThemeSettings? DesktopThemeSettings;

    public static IRecognitionClient? RecognitionClient;
    public static INotificationClient? NotificationClient;
    public static IAudioRecordingClient? RecordingClient;
    
    public static PlaylistFileWriter? PlaylistFileWriter;
    public static LastfmScrobbler? LastfmScrobbler;

    public static VRChatLogParser? VRChatLogParser;
    
    public static VXMusicOverlayInstance? VXMusicOverlay;

    public VXMusicSession(RecognitionSettings recognitionSettings, ConnectionsSettings connectionsSettings)
    {
        RecognitionSettings = recognitionSettings;
        NotificationSettings = new NotificationSettings();
        ConnectionsSettings = connectionsSettings;

        RecordingClient = GetAudioRecordingClient();
        RecognitionClient = RecognitionSettings.GetClientFromSetRecognitionApi(); //VXMusicAPI.SetRecognitionApi(recognitionSettings.CurrentRecognitionApi);
        //NotificationClient = new XSOverlay(); //VXMusicAPI.SetNotificationClient(notificationSettings.CurrentNotificationService);

        NotificationClient = App.ServiceProvider.GetRequiredService<XSOverlay>(); //VXMusicAPI.SetNotificationClient(notificationSettings.CurrentNotificationService);

        PlaylistFileWriter = App.ServiceProvider.GetRequiredService<PlaylistFileWriter>();
        LastfmScrobbler = App.ServiceProvider.GetRequiredService<LastfmScrobbler>();
        
        VRChatLogParser = App.ServiceProvider.GetRequiredService<VRChatLogParser>();

        ColourSchemeManager.SetTheme(DesktopThemeSettings.GetDesktopThemeFromSettings());

        //VXMusicOverlay = App.ServiceProvider.GetRequiredService<VXMusicOverlayInstance>();
    }

    public void SetRecognitionClient(RecognitionApi recognitionApi)
    {
        RecognitionSettings.CurrentRecognitionApi = recognitionApi;

        switch (recognitionApi)
        {
            case RecognitionApi.Shazam:
                RecognitionClient = App.ServiceProvider.GetRequiredService<ShazamClient>();
                return;
            case RecognitionApi.AudD:
                RecognitionClient = App.ServiceProvider.GetRequiredService<AudDClient>();
                return;
            default:
                Trace.WriteLine("Recognition type not found!");
                return;
        }
    }

    public void SetNotificationService(NotificationService recognitionApi)
    {
        NotificationSettings.CurrentNotificationService = recognitionApi;

        switch (recognitionApi)
        {
            case NotificationService.SteamVR:
                NotificationClient = App.ServiceProvider.GetRequiredService<VXMusicOverlayInstance>();
                return;
            case NotificationService.XSOverlay:
                NotificationClient = App.ServiceProvider.GetRequiredService<XSOverlay>();
                return;
            default:
                Trace.WriteLine("Recognition type not found!");
                return;
        }
    }

    public static IAudioRecordingClient GetAudioRecordingClient()
    {
        return App.ServiceProvider.GetRequiredService<WindowsAudioDeviceListener>();
    }
}

public class RecognitionSettings
{
    // App Settings
    public ShazamSettings ShazamSettings { get; set; }
    public AudDSettings AudDSettings { get; set; }

    // User Settings
    public RecognitionApi CurrentRecognitionApi;

    public RecognitionSettings()
    {
        CurrentRecognitionApi = GetCurrentRecognitionApiFromSettings();
    }

    public static RecognitionApi GetCurrentRecognitionApiFromSettings()
    {
        Enum.TryParse<RecognitionApi>(VXMusicDesktop.Properties.Settings.Default.RecognitionAPI, out var currentRecognitionApi);
        return currentRecognitionApi;
    }

    public static String GetRecognitionApiString()
    {
        return VXMusicDesktop.Properties.Settings.Default.RecognitionAPI;
    }

    public static void SetRecognitionApiInSettings(RecognitionApi api)
    {
        VXMusicDesktop.Properties.Settings.Default.RecognitionAPI = api.ToString();
        VXMusicDesktop.Properties.Settings.Default.Save();
    }

    public static IRecognitionClient GetClientFromSetRecognitionApi()
    {
        switch (GetCurrentRecognitionApiFromSettings())
        {
            case RecognitionApi.Shazam:
                return App.ServiceProvider.GetRequiredService<ShazamClient>();
            case RecognitionApi.AudD:
                return App.ServiceProvider.GetRequiredService<AudDClient>();
            default:
                return null;
        }
    }
}

public class ConnectionsSettings
{
    // App Settings
    public SpotifySettings SpotifySettings { get; set; }
    public LastfmSettings LastfmSettings { get; set; }

    // User Settings
    public bool IsSpotifyConnected { get; set; }
    public bool IsLastfmConnected { get; set; }

    public ConnectionsSettings()
    {
        IsSpotifyConnected = VXMusicDesktop.Properties.Settings.Default.SpotifyEnabled;
        IsLastfmConnected = VXMusicDesktop.Properties.Settings.Default.LastfmEnabled;
    }
}

public class NotificationSettings
{
    public NotificationService CurrentNotificationService;

    public NotificationSettings()
    {
        Enum.TryParse<NotificationService>(VXMusicDesktop.Properties.Settings.Default.NotificationService, out CurrentNotificationService);

    }
}

public class ShazamSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string X_RapidAPI_Key { get; set; }
    public required string X_RapidAPI_Host { get; set; }
}

public class AudDSettings
{
    public required string ClientId { get; set; }
}

public class SpotifySettings
{
    public required string ClientId { get; set; }
}

public class LastfmSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class LoggingSettings
{
    public LogLevelSettings? LogLevel { get; set; }
}

public class LogLevelSettings
{
    public string? Default { get; set; }
    public string? Microsoft { get; set; }
    public string? Microsoft_Hosting_Lifetime { get; set; }
}

public class DesktopThemeSettings
{
    public string? DesktopTheme { get; set; }

    public static DesktopTheme GetDesktopThemeFromSettings()
    {
        // Parse a string to get the enum value
        string currentThemeFromSettings = VXMusicDesktop.Properties.Settings.Default.DesktopTheme;
        
        if (Enum.TryParse<DesktopTheme>(currentThemeFromSettings, out DesktopTheme desktopTheme))
        {
            return desktopTheme;
        }
        else
        {
            throw new Exception("Unable to get Desktop Theme from string in Settings.settings");
        }
    }

    public static void SetDesktopThemeInSettings(DesktopTheme theme)
    {
        VXMusicDesktop.Properties.Settings.Default.DesktopTheme = theme.ToString();
        VXMusicDesktop.Properties.Settings.Default.Save();
    }
}