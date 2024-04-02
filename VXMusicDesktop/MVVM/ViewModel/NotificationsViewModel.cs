using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class NotificationsViewModel : INotifyPropertyChanged
    {
        public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();

        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        private RelayCommand steamVREnableButtonClick;
        private RelayCommand xsOverlayEnableButtonClick;

        private RelayCommand notificationViewLoaded;

        public ICommand SteamVREnableButtonClick => steamVREnableButtonClick ??= new RelayCommand(PerformSteamVREnableButtonClick);
        private bool _isSteamVRNotificationServiceEnabled;

        public ICommand XSOverlayEnableButtonClick => xsOverlayEnableButtonClick ??= new RelayCommand(PerformXSOverlayEnableButtonClick);
        private bool _isXSOverlayNotificationServiceEnabled;

        public ICommand NotificationViewLoaded => notificationViewLoaded ??= new RelayCommand(OnNotificationViewLoaded);


        private bool _isNotificationServiceReady = true;

        public static bool ShazamApi = true; // WHAT???

        public NotificationsViewModel(SharedViewModel sharedViewModel)
        {
            SharedViewModel = sharedViewModel;
            Initialise();
        }
        
        public void Initialise()
        {
            _isSteamVRNotificationServiceEnabled = App.VXMusicSession.NotificationSettings.CurrentNotificationService == NotificationService.SteamVR;
            _isXSOverlayNotificationServiceEnabled = App.VXMusicSession.NotificationSettings.CurrentNotificationService == NotificationService.XSOverlay;

            ProcessNotificationServiceState();
        }
        
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

        private void OnNotificationViewLoaded(object commandParameter)
        {
            ProcessNotificationServiceState();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PerformSteamVREnableButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetNotificationService(NotificationService.SteamVR);
            VXUserSettings.Notifications.SetNotificationServiceInSettings(NotificationService.SteamVR);
            ProcessNotificationServiceState();
            VXMusicSession.RaiseSteamVrNotificationEnabled();
            
            Logger.LogInformation($"Notification Service set to {App.VXMusicSession.NotificationSettings.CurrentNotificationService}");
        }

        private void PerformXSOverlayEnableButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetNotificationService(NotificationService.XSOverlay);
            VXUserSettings.Notifications.SetNotificationServiceInSettings(NotificationService.XSOverlay);
            ProcessNotificationServiceState();
            VXMusicSession.RaiseXsOverlayNotificationEnabled();
            
            Logger.LogInformation($"Notification Service set to {App.VXMusicSession.NotificationSettings.CurrentNotificationService}");
        }

        private void ProcessNotificationServiceState()
        {
            switch (App.VXMusicSession.NotificationSettings.CurrentNotificationService)
            {
                case NotificationService.SteamVR:
                    IsSteamVRNotificationServiceEnabled = true;
                    IsXSOverlayNotificationServiceEnabled = false;
                    SharedViewModel.IsSteamVrNotificationServiceEnabled = true;
                    SharedViewModel.IsXsOverlayNotificationServiceEnabled = false;
                    break;
                case NotificationService.XSOverlay:
                    IsXSOverlayNotificationServiceEnabled = true;
                    IsSteamVRNotificationServiceEnabled = false;
                    SharedViewModel.IsXsOverlayNotificationServiceEnabled = true;
                    SharedViewModel.IsSteamVrNotificationServiceEnabled = false;
                    break;
                default:
                    IsSteamVRNotificationServiceEnabled = false;
                    IsXSOverlayNotificationServiceEnabled = false;
                    break;
            }

            _isNotificationServiceReady = IsSteamVRNotificationServiceEnabled || IsXSOverlayNotificationServiceEnabled;
        }

    }
}
