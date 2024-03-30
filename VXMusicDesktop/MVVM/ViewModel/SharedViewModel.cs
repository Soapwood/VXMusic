using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VXMusicDesktop.MVVM.ViewModel;

public class SharedViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isShazamApiConnected;
    private bool _isAudDApiConnected;

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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}