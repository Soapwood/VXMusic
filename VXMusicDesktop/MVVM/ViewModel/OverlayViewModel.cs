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
    internal class OverlayViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        
        private RelayCommand launchOverlayOnStartupButtonChecked;
        public ICommand LaunchOverlayOnStartupButtonChecked => launchOverlayOnStartupButtonChecked ??= new RelayCommand(SetLaunchOverlayOnStartupEnabled);
        private RelayCommand launchOverlayOnStartupButtonUnchecked;
        public ICommand LaunchOverlayOnStartupButtonUnhecked => launchOverlayOnStartupButtonUnchecked ??= new RelayCommand(SetLaunchOverlayOnStartupDisabled);

        private bool _launchOverlayOnStartup;

        public OverlayViewModel()
        {
            _launchOverlayOnStartup = VXUserSettings.Overlay.GetCurrentOverlayLaunchOnStartup();
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
        
        private void SetLaunchOverlayOnStartupEnabled(object commandParameter)
        {
            //App.VXMusicSession.SetNotificationService(NotificationService.SteamVR);
            //NotificationSettings.SetNotificationServiceInSettings(NotificationService.SteamVR);
            //ProcessNotificationServiceState();
            //VXMusicSession.RaiseSteamVrNotificationEnabled();
            //VXMusicSession.VXMusicOverlay.

        }

        private void SetLaunchOverlayOnStartupDisabled(object commandParameter)
        {
            //App.VXMusicSession.SetNotificationService(NotificationService.XSOverlay);
            //NotificationSettings.SetNotificationServiceInSettings(NotificationService.XSOverlay);
            //ProcessNotificationServiceState();
            //VXMusicSession.RaiseXsOverlayNotificationEnabled();
            //VXMusicSession.VXMusicOverlay.SetRightHandOverlayTrackedDevice();

        }
    }
}
