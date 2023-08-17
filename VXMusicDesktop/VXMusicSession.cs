using System;
using System.ComponentModel;
using VXMusic;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;

//using System.Windows.Forms.PropertyGridInternal;

namespace VXMusicDesktop;

public class VXMusicSession
{
    // Current Recognition Options
    // Spotify Connected
    // Notification Options

    public RecognitionSettings? RecognitionSettings;
    public NotificationSettings? NotificationSettings;
    public ConnectionsSettings? ConnectionsSettings;

    public IRecognitionClient? RecognitionClient;
    public INotificationClient? NotificationClient;

    public void InitialiseVXMusicSession()
    {
        RecognitionSettings = new RecognitionSettings();
        NotificationSettings = new NotificationSettings();
        ConnectionsSettings = new ConnectionsSettings();

        RecognitionClient = RecognitionSettings.GetClientFromSetRecognitionApi(); //VXMusicAPI.SetRecognitionApi(recognitionSettings.CurrentRecognitionApi);
        NotificationClient = new XSOverlay(); //VXMusicAPI.SetNotificationClient(notificationSettings.CurrentNotificationService);
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
        CurrentRecognitionApi = GetRecognitionApi();
    }

    public static RecognitionApi GetRecognitionApi()
    {
        Enum.TryParse<RecognitionApi>(VXMusicDesktop.Properties.Settings.Default.RecognitionAPI, out var currentRecognitionApi);
        return currentRecognitionApi;
    }

    public static String GetRecognitionApiString()
    {
        return VXMusicDesktop.Properties.Settings.Default.RecognitionAPI;
    }

    public static void SetRecognitionApi(RecognitionApi api)
    {
        VXMusicDesktop.Properties.Settings.Default.RecognitionAPI = api.ToString();
    }

    public static IRecognitionClient GetClientFromSetRecognitionApi()
    {
        switch (GetRecognitionApi())
        {
            case RecognitionApi.Shazam:
                return new ShazamClient();
            case RecognitionApi.AudD:
                return new AudDClient();
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