using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusic;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class OverlayViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private RelayCommand launchOverlayOnStartupToggleButton;
        public ICommand LaunchOverlayOnStartupToggleButton => launchOverlayOnStartupToggleButton ??= new RelayCommand(SetLaunchOverlayOnStartup);

        private bool _launchOverlayOnStartup;

        public OverlayViewModel()
        {
            LaunchOverlayOnStartup = VXUserSettings.Overlay.GetCurrentOverlayLaunchOnStartup();
        }
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public bool LaunchOverlayOnStartup
        {
            get { return _launchOverlayOnStartup; }
            set
            {
                if (_launchOverlayOnStartup != value)
                {
                    _launchOverlayOnStartup = value;
                    OnPropertyChanged(nameof(LaunchOverlayOnStartup));
                }
            }
        }
        
        public void SetLaunchOverlayOnStartup(object commandParameter)
        {
            VXUserSettings.Overlay.SetLaunchOverlayOnStartup(LaunchOverlayOnStartup);
        }
    }
}
