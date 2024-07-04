using System.ComponentModel;
using System.Windows.Input;
using VXMusic.Overlay;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class OverlayViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private RelayCommand launchOverlayOnStartupToggleButton;
        private RelayCommand enableOverlayOnLeftHand;
        private RelayCommand enableOverlayOnRightHand;
        private RelayCommand askForUpdatesToggleButton;

        public ICommand LaunchOverlayOnStartupToggleButton =>
            launchOverlayOnStartupToggleButton ??= new RelayCommand(SetLaunchOverlayOnStartup);

        public ICommand EnableOverlayOnLeftHand =>
            enableOverlayOnLeftHand ??= new RelayCommand(SetEnableOverlayOnLeftHand);

        public ICommand EnableOverlayOnRightHand =>
            enableOverlayOnRightHand ??= new RelayCommand(SetEnableOverlayOnRightHand);
        
        public ICommand AskForUpdatesOnStartupToggleButton =>
            askForUpdatesToggleButton ??= new RelayCommand(SetAskForUpdatesOnStartup);

        private bool _launchOverlayOnStartup;
        private bool _overlayEnabledOnLeftHand;
        private bool _overlayEnabledOnRightHand;
        private bool _askForUpdatesOnStartup;


        public OverlayViewModel()
        {
            LaunchOverlayOnStartup = VXUserSettings.Overlay.GetCurrentOverlayLaunchOnStartup();
            OverlayEnabledOnLeftHand = VXUserSettings.Overlay.GetOverlayAnchor() == VXMusicOverlayAnchor.LeftHand;
            OverlayEnabledOnRightHand = VXUserSettings.Overlay.GetOverlayAnchor() == VXMusicOverlayAnchor.RightHand;
            AskForUpdatesOnStartup = VXUserSettings.Settings.GetAskForUpdatesOnStartup();
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

        public bool OverlayEnabledOnLeftHand
        {
            get { return _overlayEnabledOnLeftHand; }
            set
            {
                if (_overlayEnabledOnLeftHand != value)
                {
                    _overlayEnabledOnLeftHand = value;
                    OverlayEnabledOnRightHand = !value;
                    OnPropertyChanged(nameof(OverlayEnabledOnLeftHand));
                }
            }
        }

        public bool OverlayEnabledOnRightHand
        {
            get { return _overlayEnabledOnRightHand; }
            set
            {
                if (_overlayEnabledOnRightHand != value)
                {
                    _overlayEnabledOnRightHand = value;
                    OverlayEnabledOnLeftHand = !value;
                    OnPropertyChanged(nameof(OverlayEnabledOnRightHand));
                }
            }
        }
        
        public bool AskForUpdatesOnStartup
        {
            get { return _askForUpdatesOnStartup; }
            set
            {
                if (_askForUpdatesOnStartup != value)
                {
                    _askForUpdatesOnStartup = value;
                    OnPropertyChanged(nameof(AskForUpdatesOnStartup));
                }
            }
        }

        public void SetLaunchOverlayOnStartup(object commandParameter)
        {
            VXUserSettings.Overlay.SetLaunchOverlayOnStartup(LaunchOverlayOnStartup);
        }

        public void SetEnableOverlayOnLeftHand(object commandParameter)
        {
            VXMusicOverlayInterface.SendOverlayAnchorUpdateRequest(
                VXMMessage.ENABLE_OVERLAY_ANCHOR_LEFTHAND_REQUEST);
        }

        public void SetEnableOverlayOnRightHand(object commandParameter)
        {
            VXMusicOverlayInterface.SendOverlayAnchorUpdateRequest(VXMMessage.ENABLE_OVERLAY_ANCHOR_RIGHTHAND_REQUEST);
        }
        
        public void SetAskForUpdatesOnStartup(object commandParameter)
        {
            VXUserSettings.Settings.SetAskForUpdatesOnStartup(_askForUpdatesOnStartup);
        }
    }
}