using System;
using System.ComponentModel;
using VXMusic;

//using System.Windows.Forms.PropertyGridInternal;

namespace VXMusicDesktop;

public class VXMusicSession
{
    // Current Recognition Options
    // Spotify Connected
    // Notification Options

    public RecognitionSettings? RecognitionSettings;
    public NotificationSettings? NotificationSettings;
    public SpotifySettings? SpotifySettings;

    public IRecognitionClient? RecognitionClient;
    public INotificationClient? NotificationClient;

    public void InitialiseVXMusicSession()
    {
        RecognitionSettings = new RecognitionSettings();
        NotificationSettings = new NotificationSettings();
        SpotifySettings = new SpotifySettings();

        RecognitionClient = null; //VXMusicAPI.SetRecognitionApi(recognitionSettings.CurrentRecognitionApi);
        NotificationClient = new XSOverlay(); //VXMusicAPI.SetNotificationClient(notificationSettings.CurrentNotificationService);
    }
}

public class RecognitionSettings
{
    public RecognitionApi CurrentRecognitionApi; // Get from User/Application Settings?

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
}

public class SpotifySettings
{
    public bool IsSpotifyConnected;

    public SpotifySettings()
    {
        IsSpotifyConnected = VXMusicDesktop.Properties.Settings.Default.SpotifyEnabled;
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