using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using VXMusic;
using VXMusic.Audio;
using VXMusic.Audio.Recording;
using VXMusic.FileWriter;
using VXMusic.Lastfm.Scrobbling;
using VXMusic.LogParser.VRChat;
using VXMusic.Overlay;
using VXMusic.OVRToolkit;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;
using VXMusic.Spotify.Authentication;
using VXMusic.Tidal.Authentication;
using VXMusic.VRChat;
using VXMusicDesktop.Core;
using VXMusicDesktop.MVVM.ViewModel;
using VXMusicDesktop.Theme;
using VXMusicDesktop.Toast;
using VXMusicDesktop.Update;

//using System.Windows.Forms.PropertyGridInternal;

namespace VXMusicDesktop;

public class VXMusicSession
{
    // Current Recognition Options
    // Spotify Connected
    // Notification Options
    
    public RecognitionSettings RecognitionSettings;
    public NotificationSettings NotificationSettings;
    public OverlaySettings OverlaySettings;
    public ConnectionsSettings ConnectionsSettings;
    public static DesktopThemeSettings? DesktopThemeSettings;

    public static IRecognitionClient? RecognitionClient;
    public static INotificationClient? NotificationClient;
    public static IAudioRecordingClient? RecordingClient;
    
    public VrChatOscNotificationClient? VrChatNotification;
    public ToastNotificationClient? ToastNotification;
    
    public static PlaylistFileWriter? PlaylistFileWriter;
    public static LastfmScrobbler? LastfmScrobbler;

    public VRChatLogParser VRChatLogParser;
    public SteamVROverlayAppsInterface SteamVrOverlayApps;
    public VXMusicUpdateHandler VxMusicUpdate;
    public VXMusicSettingsSyncHandler VXMusicSettingsSync;
    public RegistryInterface RegistryInterface;
    
    public static SharedViewModel SharedViewModel { get; set; }
    
    public static event EventHandler LastFmLogin;
    //public static event EventHandler SpotifyLogin;

    public static event EventHandler SteamVrNotificationEnabled;
    public static event EventHandler XsOverlayNotificationEnabled;
    public static event EventHandler OvrToolkitNotificationEnabled;

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
        
        VrChatNotification = NotificationSettings.GetVRChatNotificationClient();
        
        PlaylistFileWriter = App.ServiceProvider.GetRequiredService<PlaylistFileWriter>();
        LastfmScrobbler = App.ServiceProvider.GetRequiredService<LastfmScrobbler>();
        
        VRChatLogParser = App.ServiceProvider.GetRequiredService<VRChatLogParser>();
        
        SteamVrOverlayApps = App.ServiceProvider.GetRequiredService<SteamVROverlayAppsInterface>();
        
        VxMusicUpdate = App.ServiceProvider.GetRequiredService<VXMusicUpdateHandler>();
        VXMusicSettingsSync = App.ServiceProvider.GetRequiredService<VXMusicSettingsSyncHandler>();
        RegistryInterface = App.ServiceProvider.GetRequiredService<RegistryInterface>();

        ColourSchemeManager.SetTheme(VXUserSettings.Desktop.GetCurrentDesktopTheme());

        SpotifyAuthentication.CredentialsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions", "VXMusic", "LocalAuth", "Spotify");
        SpotifyAuthentication.CredentialsFilePath = Path.Combine(SpotifyAuthentication.CredentialsPath, "credentials.json");
        SpotifyAuthentication.ClientId = ConnectionsSettings.SpotifySettings.ClientId;
        
        TidalAuthentication.CredentialsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions", "VXMusic", "LocalAuth", "Tidal");
        TidalAuthentication.CredentialsFilePath = Path.Combine(TidalAuthentication.CredentialsPath, "credentials_t.json");
        TidalAuthentication.ClientId = ConnectionsSettings.TidalSettings.ClientId;

        if (OverlaySettings.LaunchOverlayOnStartup)
            App.VXMOverlayProcess = VXMusicOverlayInterface.LaunchVXMOverlayRuntime(OverlaySettings.RuntimePath);
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
        NotificationSettings.CurrentVRNotificationService = recognitionApi;

        switch (recognitionApi)
        {
            case NotificationService.SteamVR:
                NotificationClient = App.ServiceProvider.GetRequiredService<SteamVRNotificationClient>();
                return;
            case NotificationService.XSOverlay:
                NotificationClient = App.ServiceProvider.GetRequiredService<XsOverlayNotificationClient>();
                return;
            case NotificationService.OVRToolkit:
                NotificationClient = App.ServiceProvider.GetRequiredService<OvrToolkitNotificationClient>();
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
    
    public static void RaiseOvrToolkitNotificationEnabled()
    {
        OvrToolkitNotificationEnabled?.Invoke(null, EventArgs.Empty);
    }
}

public class OverlaySettings
{
    public string RuntimePath;
    public bool LaunchOverlayOnStartup;
    public VXMusicOverlayAnchor OverlayAnchor;

    public OverlaySettings()
    {
        LaunchOverlayOnStartup = VXUserSettings.Overlay.GetCurrentOverlayLaunchOnStartup();
        OverlayAnchor = VXUserSettings.Overlay.GetOverlayAnchor();
    }

    public string ToPayload()
    {
        string overlaySettingsPayload = "";

        overlaySettingsPayload += OverlayAnchor == VXMusicOverlayAnchor.LeftHand
            ? VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_REQUEST
            : VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_REQUEST;
        
        return overlaySettingsPayload;
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
    public TidalSettings TidalSettings { get; set; }
    public LastfmSettings LastfmSettings { get; set; }
    public bool IsLastfmConnected { get; set; }
    public bool IsVrChatConnected { get; set; }

    public ConnectionsSettings()
    {

    }
}

public class NotificationSettings
{
    public NotificationService CurrentVRNotificationService;
    public bool IsVRChatNotificationServiceEnabled;

    public NotificationSettings()
    {
        CurrentVRNotificationService = VXUserSettings.Notifications.GetCurrentNotificationService();
        IsVRChatNotificationServiceEnabled = VXUserSettings.Notifications.GetIsVRChatNotificationsEnabled();
    }

    public static INotificationClient? GetNotificationServiceFromSetConfiguration()
    {
        switch (VXUserSettings.Notifications.GetCurrentNotificationService())
        {
            case NotificationService.SteamVR:
                return App.ServiceProvider.GetRequiredService<SteamVRNotificationClient>();
            case NotificationService.XSOverlay:
                return App.ServiceProvider.GetRequiredService<XsOverlayNotificationClient>();
            case NotificationService.OVRToolkit:
                return App.ServiceProvider.GetRequiredService<OvrToolkitNotificationClient>();
            default:
                return null;
        }
    }
    
    public static VrChatOscNotificationClient GetVRChatNotificationClient()
    {
        return App.ServiceProvider.GetRequiredService<VrChatOscNotificationClient>();
    }
}

public class ShazamSettings
{
    // Legacy API fields - kept for backward compatibility but not used in keyless mode
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? X_RapidAPI_Key { get; set; }
    public string? X_RapidAPI_Host { get; set; }

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
    public PlaylistSaveSettings PlaylistSavingSaveSetting { get; set; }
}

public class TidalSettings
{
    public required string ClientId { get; set; }
    public PlaylistSaveSettings PlaylistSavingSaveSetting { get; set; }
}

public class LastfmSettings
{
    public required string ClientId { get; set; }
    public required string AppToken { get; set; }
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