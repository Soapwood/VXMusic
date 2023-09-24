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

//public class ColourSchemeManager
//{
//    public static SolidColorBrush PrimaryColour { get; set; } 
//    public static SolidColorBrush SecondaryColour { get; set; } 
//    public static SolidColorBrush Accent1Colour { get; set; }
//    public static SolidColorBrush Accent2Colour { get; set; } 
//    public static SolidColorBrush TextBasic { get; set; }

//    public static readonly string Darkmode1Primary = "#252525";
//    public static readonly string Darkmode1Secondary = "#6930C3";
//    public static readonly string Darkmode1Accent1Colour = "#64DFDF";
//    public static readonly string Darkmode1Accent2Colour = "#80FFDB";
//    public static readonly string Darkmode1TextBasic = "#F8F0E3";

//    public static readonly string Darkmode2Primary = "#000000";
//    public static readonly string Darkmode2Secondary = "#52057B";
//    public static readonly string Darkmode2Accent1Colour = "#892CDC";
//    public static readonly string Darkmode2Accent2Colour = "#BC6FF1";
//    public static readonly string Darkmode2TextBasic = "#F8F0E3";

//    public static void SetThemeDarkmode1()
//    {
//        PrimaryColour = new SolidColorBrush(FromHex(Darkmode1Primary));
//        SecondaryColour = new SolidColorBrush(FromHex(Darkmode1Secondary));
//        Accent1Colour = new SolidColorBrush(FromHex(Darkmode1Accent1Colour));
//        Accent2Colour = new SolidColorBrush(FromHex(Darkmode1Accent2Colour));
//        TextBasic = new SolidColorBrush(FromHex(Darkmode1TextBasic));
//    }

//    public static void SetThemeDarkmode2()
//    {
//        PrimaryColour = new SolidColorBrush(FromHex(Darkmode2Primary));
//        SecondaryColour = new SolidColorBrush(FromHex(Darkmode2Secondary));
//        Accent1Colour = new SolidColorBrush(FromHex(Darkmode2Accent1Colour));
//        Accent2Colour = new SolidColorBrush(FromHex(Darkmode2Accent2Colour));
//        TextBasic = new SolidColorBrush(FromHex(Darkmode2TextBasic));
//    }
//    public static Color FromHex(string hex)
//    {
//        hex = hex.TrimStart('#'); // Remove '#' if present
//        if (hex.Length != 6)
//            throw new ArgumentException("Hex color code must be 6 characters long.", nameof(hex));

//        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
//        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
//        byte b = Convert.ToByte(hex.Substring(4, 2), 16);

//        return Color.FromRgb(r, g, b);
//    }
//}
public class VXMusicSession
{
    // Current Recognition Options
    // Spotify Connected
    // Notification Options
    
    public RecognitionSettings? RecognitionSettings;
    public static NotificationSettings? NotificationSettings;
    public static ConnectionsSettings? ConnectionsSettings;

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

        ColourSchemeManager.SetThemeDarkmode1();

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