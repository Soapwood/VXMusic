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
    internal class NotificationsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private RelayCommand steamVREnableButtonClick;
        private RelayCommand xsOverlayEnableButtonClick;

        public ICommand SteamVREnableButtonClick => steamVREnableButtonClick ??= new RelayCommand(PerformSteamVREnableButtonClick);
        private bool _isSteamVRNotificationServiceEnabled = true;

        public ICommand XSOverlayEnableButtonClick => xsOverlayEnableButtonClick ??= new RelayCommand(PerformXSOverlayEnableButtonClick);
        private bool _isXSOverlayNotificationServiceEnabled;

        private bool _isNotificationServiceReady = true;

        public static bool ShazamApi = true; // WHAT???


        public bool IsSteamVRNotificationServiceEnabled
        {
            get { return _isSteamVRNotificationServiceEnabled; }
            set
            {
                _isSteamVRNotificationServiceEnabled = value;
                OnPropertyChanged(nameof(IsSteamVRNotificationServiceEnabled));
            }
        }

        public bool IsXSOverlayNotificationServiceEnabled
        {
            get { return _isXSOverlayNotificationServiceEnabled; }
            set
            {
                _isXSOverlayNotificationServiceEnabled = value;
                OnPropertyChanged(nameof(IsXSOverlayNotificationServiceEnabled));
            }
        }

        public bool IsNotificationServiceReady
        {
            get { return _isNotificationServiceReady; }
            set
            {
                _isNotificationServiceReady = value;
                OnPropertyChanged(nameof(IsNotificationServiceReady));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PerformSteamVREnableButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetNotificationService(NotificationService.SteamVR);
            ProcessRecognitionApiState(NotificationService.SteamVR);
        }

        private void PerformXSOverlayEnableButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetNotificationService(NotificationService.XSOverlay);
            ProcessRecognitionApiState(NotificationService.XSOverlay);
        }

        private void ProcessRecognitionApiState(NotificationService incomingService)
        {
            switch (incomingService)
            {
                case NotificationService.SteamVR:
                    IsSteamVRNotificationServiceEnabled = true;
                    IsXSOverlayNotificationServiceEnabled = false;
                    break;
                case NotificationService.XSOverlay:
                    IsXSOverlayNotificationServiceEnabled = true;
                    IsSteamVRNotificationServiceEnabled = false;
                    break;
                default:
                    IsSteamVRNotificationServiceEnabled = false;
                    IsSteamVRNotificationServiceEnabled = false;
                    break;
            }

            _isNotificationServiceReady = IsSteamVRNotificationServiceEnabled || IsXSOverlayNotificationServiceEnabled;
        }

    }
}
