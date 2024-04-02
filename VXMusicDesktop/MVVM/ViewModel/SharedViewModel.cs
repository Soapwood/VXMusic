using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using VXMusic;
using VXMusic.LogParser.VRChat;

namespace VXMusicDesktop.MVVM.ViewModel;

public class SharedViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public SharedViewModel()
    {
        App.VXMusicSession.VRChatLogParser.PropertyChanged += VRChatLogParserIsVrChatSessionRunningPropertyChanged;
    }
    
    // Recognition Shared Fields
    private bool _isShazamApiConnected;
    private bool _isAudDApiConnected;

    // Notification Shared Fields
    private bool _isSteamVrNotificationServiceEnabled;
    private bool _isXsOverlayNotificationServiceEnabled;

    // Connection Shared Fields
    private bool _isSpotifyConnected;
    private bool _isLastFmConnected;
    
    // Game Client Shared Fields
    private bool _isVrChatConnected;
    
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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    private void VRChatLogParserIsVrChatSessionRunningPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.VXMusicSession.VRChatLogParser.IsVrChatSessionRunning))
        {
            // The IsVRChatSessionRunning property has changed
            OnVRChatSessionRunningChanged(App.VXMusicSession.VRChatLogParser.IsVrChatSessionRunning);
        }
    }
    
    private void OnVRChatSessionRunningChanged(bool isVrChatConnected)
    {
        IsVrChatConnected = isVrChatConnected;
    }

}