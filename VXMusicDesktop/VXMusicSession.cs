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
using VXMusicDesktop.MVVM.ViewModel;
using VXMusic.Spotify.Authentication;

//using System.Windows.Forms.PropertyGridInternal;

namespace VXMusicDesktop;

public class VXMusicSession
{
    // Current Recognition Options
    // Spotify Connected
    // Notification Options
    
    public RecognitionSettings RecognitionSettings;
    public NotificationSettings NotificationSettings;
    public static OverlaySettings OverlaySettings;
    public ConnectionsSettings ConnectionsSettings;
    public static DesktopThemeSettings? DesktopThemeSettings;

    public static IRecognitionClient? RecognitionClient;
    public static INotificationClient? NotificationClient;
    public static IAudioRecordingClient? RecordingClient;
    
    public static PlaylistFileWriter? PlaylistFileWriter;
    public static LastfmScrobbler? LastfmScrobbler;

    public VRChatLogParser VRChatLogParser;
    
    public static VXMusicOverlayInstance? VXMusicOverlay;

    public static event EventHandler LastFmLogin;
    //public static event EventHandler SpotifyLogin;

    public static event EventHandler SteamVrNotificationEnabled;
    public static event EventHandler XsOverlayNotificationEnabled;

    public VXMusicSession(RecognitionSettings recognitionSettings, ConnectionsSettings connectionsSettings, OverlaySettings overlaySettings)
    {
        RecognitionSettings = recognitionSettings;
        NotificationSettings = new NotificationSettings();
        ConnectionsSettings = connectionsSettings;
        OverlaySettings = overlaySettings;
    }

    public void Initialise()
    {
        RecordingClient = GetAudioRecordingClient();
        RecognitionClient = RecognitionSettings.GetClientFromSetRecognitionApi(); //VXMusicAPI.SetRecognitionApi(recognitionSettings.CurrentRecognitionApi);
        
        //NotificationClient = new XSOverlay(); //VXMusicAPI.SetNotificationClient(notificationSettings.CurrentNotificationService);

        NotificationClient = NotificationSettings.GetNotificationServiceFromSetConfiguration(); //VXMusicAPI.SetNotificationClient(notificationSettings.CurrentNotificationService);

        PlaylistFileWriter = App.ServiceProvider.GetRequiredService<PlaylistFileWriter>();
        LastfmScrobbler = App.ServiceProvider.GetRequiredService<LastfmScrobbler>();
        
        VRChatLogParser = App.ServiceProvider.GetRequiredService<VRChatLogParser>();

        ColourSchemeManager.SetTheme(VXUserSettings.Desktop.GetCurrentDesktopTheme());

        SpotifyAuthentication.ClientId = ConnectionsSettings.SpotifySettings.ClientId;

        if (OverlaySettings.LaunchOverlayOnStartup)
        {
            App.VXMOverlayProcessId = VXMusicOverlayInterface.LaunchVXMOverlayRuntime(OverlaySettings.RuntimePath);
            MainViewModel.InitialiseOverlayHeartbeatMonitor();
        }
        //VXListenForOverlayMessage();
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
                NotificationClient = App.ServiceProvider.GetRequiredService<SteamVRNotificationClient>();
                return;
            case NotificationService.XSOverlay:
                NotificationClient = App.ServiceProvider.GetRequiredService<XSOverlay>();
                return;
            default:
                Trace.WriteLine("Notification service type not found!");
                return;
        }
    }
    
    public static IAudioRecordingClient GetAudioRecordingClient()
    {
        return App.ServiceProvider.GetRequiredService<WindowsAudioDeviceListener>();
    }

    public static void RaiseLastFmLoggedIn()
    {
        LastFmLogin?.Invoke(null, EventArgs.Empty);
    }

    public static void RaiseSteamVrNotificationEnabled()
    {
        SteamVrNotificationEnabled?.Invoke(null, EventArgs.Empty);
    }

    public static void RaiseXsOverlayNotificationEnabled()
    {
        XsOverlayNotificationEnabled?.Invoke(null, EventArgs.Empty);
    }
}

public class OverlaySettings
{
    public string RuntimePath;
    public bool LaunchOverlayOnStartup;

    public OverlaySettings()
    {
        LaunchOverlayOnStartup = VXUserSettings.Overlay.GetCurrentOverlayLaunchOnStartup();
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
        CurrentRecognitionApi = VXUserSettings.Recognition.GetCurrentRecognitionApi();
    }
    
    public static IRecognitionClient GetClientFromSetRecognitionApi()
    {
        switch (VXUserSettings.Recognition.GetCurrentRecognitionApi())
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
    //public bool IsSpotifyConnected { get; set; }
    public bool IsLastfmConnected { get; set; }

    public ConnectionsSettings()
    {
        //IsSpotifyConnected = VXMusicDesktop.Properties.Settings.Default.SpotifyEnabled;
        //IsLastfmConnected = VXUserSettings.Connections.GetLastfmConnected();
    }
}

public class NotificationSettings
{
    public NotificationService CurrentNotificationService;

    public NotificationSettings()
    {
        CurrentNotificationService = VXUserSettings.Notifications.GetCurrentNotificationService();
    }

    public static INotificationClient GetNotificationServiceFromSetConfiguration()
    {
        switch (VXUserSettings.Notifications.GetCurrentNotificationService())
        {
            case NotificationService.SteamVR:
                return App.ServiceProvider.GetRequiredService<SteamVRNotificationClient>();
            case NotificationService.XSOverlay:
                return App.ServiceProvider.GetRequiredService<XSOverlay>();
            default:
                return null;
        }
    }
}

public class ShazamSettings
{
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
    public required string X_RapidAPI_Key { get; set; }
    public required string X_RapidAPI_Host { get; set; }

    public bool IsByoApiEnabled;
    public string? ByoApiKey;
}

public class AudDSettings
{
    public required string ClientId { get; set; }
    
    public bool IsByoApiEnabled;
    public string? ByoApiKey;
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
}