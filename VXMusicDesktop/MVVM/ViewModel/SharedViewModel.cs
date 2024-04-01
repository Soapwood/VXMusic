using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VXMusicDesktop.MVVM.ViewModel;

public class SharedViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Recognition Shared Fields
    private bool _isShazamApiConnected;
    private bool _isAudDApiConnected;

    // Connection Shared Fields
    private bool _isSpotifyConnected;
    
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
    
    public bool IsSpotifyConnected
    {
        get { return _isSpotifyConnected; }
        set
        {
            _isSpotifyConnected = value;
            OnPropertyChanged(nameof(IsSpotifyConnected));
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}