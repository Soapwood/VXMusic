using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic;
using VXMusic.LogParser.VRChat;
using VXMusic.Overlay;

namespace VXMusicDesktop.MVVM.ViewModel;

public class SharedViewModel : INotifyPropertyChanged
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();
    public event PropertyChangedEventHandler? PropertyChanged;

    public SharedViewModel()
    {
        App.VXMusicSession.VRChatLogParser.PropertyChanged += VRChatLogParserIsVrChatSessionRunningPropertyChanged;
        App.VXMusicSession.VRChatLogParser.PropertyChanged += VRChatLogParserCurrentVrChatWorldPropertyChanged;
    }
    
    // Recognition Shared Fields
    private bool _isShazamApiConnected;
    private bool _isAudDApiConnected;

    private bool _isRecognitionRunning;

    // Notification Shared Fields
    private bool _isSteamVrNotificationServiceEnabled;
    private bool _isXsOverlayNotificationServiceEnabled;

    // Connection Shared Fields
    private bool _isSpotifyConnected;
    private bool _isLastFmConnected;
    
    // Game Client Shared Fields
    private bool _isVrChatConnected;
    private string _currentVrChatWorld;
    
    // Overlay Shared Fields
    private bool _isOverlayRunning;

    public bool IsShazamApiConnected
    {
        get { return _isShazamApiConnected; }
        set
        {
            _isShazamApiConnected = value;
            OnPropertyChanged(nameof(IsShazamApiConnected));
        }
    }
    
    public bool IsAudDApiConnected
    {
        get { return _isAudDApiConnected; }
        set
        {
            _isAudDApiConnected = value;
            OnPropertyChanged(nameof(IsAudDApiConnected));
        }
    }
    
    public bool IsSteamVrNotificationServiceEnabled
    {
        get { return _isSteamVrNotificationServiceEnabled; }
        set
        {
            _isSteamVrNotificationServiceEnabled = value;
            OnPropertyChanged(nameof(IsSteamVrNotificationServiceEnabled));
        }
    }
    
    public bool IsXsOverlayNotificationServiceEnabled
    {
        get { return _isXsOverlayNotificationServiceEnabled; }
        set
        {
            _isXsOverlayNotificationServiceEnabled = value;
            OnPropertyChanged(nameof(IsXsOverlayNotificationServiceEnabled));
        }
    }
    
    public bool IsSpotifyConnected
    {
        get { return _isSpotifyConnected; }
        set
        {
            _isSpotifyConnected = value;
            OnPropertyChanged(nameof(IsSpotifyConnected));
        }
    }
    
    public bool IsLastFmConnected
    {
        get { return _isLastFmConnected; }
        set
        {
            _isLastFmConnected = value;
            OnPropertyChanged(nameof(IsLastFmConnected));
        }
    }
    
    public bool IsVrChatConnected
    {
        get { return _isVrChatConnected; }
        set
        {
            _isVrChatConnected = value;
            OnPropertyChanged(nameof(IsVrChatConnected));
        }
    }
    
    public string CurrentVrChatWorld
    {
        get { return _currentVrChatWorld; }
        set
        {
            _currentVrChatWorld = value;
            OnPropertyChanged(nameof(CurrentVrChatWorld));
        }
    }
    
    public bool IsOverlayRunning
    {
        get { return _isOverlayRunning; }
        set
        {
            _isOverlayRunning = value;
            OnPropertyChanged(nameof(IsOverlayRunning));

            // Alert system if overlay has timed out.
            OnVXMusicOverlayRunningChanged();
        }
    }
    
    public bool IsRecognitionRunning
    {
        get { return _isRecognitionRunning; }
        set
        {
            _isRecognitionRunning = value;
            OnPropertyChanged(nameof(IsRecognitionRunning));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnVXMusicOverlayRunningChanged()
    {
        if (!IsRecognitionRunning && !IsOverlayRunning && VXMusicOverlayInterface.OverlayWasRunning)
        {
            Logger.LogError("VXMusicOverlay has timed out!");
            //VXMusicSession.NotificationClient.SendNotification("VXMusic Overlay has disconnected.", "", 4);
            VXMusicOverlayInterface.OverlayWasRunning = false;
        }
    }
    
    private void VRChatLogParserIsVrChatSessionRunningPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.VXMusicSession.VRChatLogParser.IsVrChatSessionRunning))
        {
            // The IsVRChatSessionRunning property has changed
            OnVRChatSessionRunningChanged(App.VXMusicSession.VRChatLogParser.IsVrChatSessionRunning);
        }
    }
    
    private void VRChatLogParserCurrentVrChatWorldPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.VXMusicSession.VRChatLogParser.CurrentVrChatWorld))
        {
            // The IsVRChatSessionRunning property has changed
            OnCurrentVrChatWorldChanged(App.VXMusicSession.VRChatLogParser.CurrentVrChatWorld);
        }
    }
    
    private void OnVRChatSessionRunningChanged(bool isVrChatConnected)
    {
        IsVrChatConnected = isVrChatConnected;
    }
    
    private void OnCurrentVrChatWorldChanged(string currentVrChatWorld)
    {
        CurrentVrChatWorld = currentVrChatWorld;
    }

}