using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic;
using VXMusic.Overlay;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class NotificationsViewModel : INotifyPropertyChanged
    {
        public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<NotificationsViewModel>>();

        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private RelayCommand notificationViewLoaded;

        private RelayCommand steamVREnableButtonClick;
        public ICommand SteamVREnableButtonClick => steamVREnableButtonClick ??= new RelayCommand(PerformSteamVREnableButtonClick);
        
        private RelayCommand xsOverlayEnableButtonClick;
        public ICommand XSOverlayEnableButtonClick => xsOverlayEnableButtonClick ??= new RelayCommand(PerformXSOverlayEnableButtonClick);
        
        private RelayCommand ovrToolkitEnableButtonClick;
        public ICommand OVRToolkitEnableButtonClick => ovrToolkitEnableButtonClick ??= new RelayCommand(PerformOvrToolkitButtonClick);
        
        private RelayCommand vrChatEnableButtonClick;
        public ICommand VRChatEnableButtonClick => vrChatEnableButtonClick ??= new RelayCommand(PerformVRChatButtonClick);

        private bool _isVRChatNotificationServiceEnabled;

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
            SharedViewModel.IsSteamVrNotificationServiceEnabled = App.VXMusicSession.NotificationSettings.CurrentVRNotificationService == NotificationService.SteamVR;
            SharedViewModel.IsXsOverlayNotificationServiceEnabled = App.VXMusicSession.NotificationSettings.CurrentVRNotificationService == NotificationService.XSOverlay;
            SharedViewModel.IsOvrToolkitNotificationServiceEnabled = App.VXMusicSession.NotificationSettings.CurrentVRNotificationService == NotificationService.OVRToolkit;
            SharedViewModel.IsVRChatNotificationServiceEnabled = App.VXMusicSession.NotificationSettings.IsVRChatNotificationServiceEnabled;

            ProcessNotificationServiceState();
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
            if (!SteamVRNotificationClient.IsSteamVrRunning())
            {
                Logger.LogWarning($"SteamVR is not running. Will not enable to SteamVR Notifications.");
                MessageBox.Show("SteamVR is not running!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            App.VXMusicSession.SetNotificationService(NotificationService.SteamVR);
            VXUserSettings.Notifications.SetNotificationServiceInSettings(NotificationService.SteamVR);
            ProcessNotificationServiceState();
            VXMusicSession.RaiseSteamVrNotificationEnabled();
            
            Logger.LogInformation($"Notification Service set to {App.VXMusicSession.NotificationSettings.CurrentVRNotificationService}");
        }

        private void PerformXSOverlayEnableButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetNotificationService(NotificationService.XSOverlay);
            VXUserSettings.Notifications.SetNotificationServiceInSettings(NotificationService.XSOverlay);
            ProcessNotificationServiceState();
            VXMusicSession.RaiseXsOverlayNotificationEnabled();
            
            Logger.LogInformation($"Notification Service set to {App.VXMusicSession.NotificationSettings.CurrentVRNotificationService}");
        }
        
        private void PerformOvrToolkitButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetNotificationService(NotificationService.OVRToolkit);
            VXUserSettings.Notifications.SetNotificationServiceInSettings(NotificationService.OVRToolkit);
            ProcessNotificationServiceState();
            VXMusicSession.RaiseXsOverlayNotificationEnabled();
            
            Logger.LogInformation($"Notification Service set to {App.VXMusicSession.NotificationSettings.CurrentVRNotificationService}");
        }
        
        private void PerformVRChatButtonClick(object commandParameter)
        {
            bool isVrChatNotificationsEnabled = VXUserSettings.Notifications.GetIsVRChatNotificationsEnabled();
            
            VXUserSettings.Notifications.SetIsVRChatNotificationsEnabled(!isVrChatNotificationsEnabled);
            App.VXMusicSession.NotificationSettings.IsVRChatNotificationServiceEnabled = !isVrChatNotificationsEnabled;
            SharedViewModel.IsVRChatNotificationServiceEnabled = !isVrChatNotificationsEnabled;

            // TODO Move this out from here by triggering event and disconnecting there
            if (SharedViewModel.IsVRChatNotificationServiceEnabled)
            {
                App.VXMusicSession.VrChatNotification.Connect();
            }
            else
            {
                App.VXMusicSession.VrChatNotification.Disconnect();
            }

            Logger.LogInformation($"VRChat Notification Service Enabled set to {App.VXMusicSession.NotificationSettings.IsVRChatNotificationServiceEnabled}");
        }

        private void ProcessNotificationServiceState()
        {
            switch (App.VXMusicSession.NotificationSettings.CurrentVRNotificationService)
            {
                case NotificationService.SteamVR:
                    SharedViewModel.IsSteamVrNotificationServiceEnabled = true;
                    SharedViewModel.IsXsOverlayNotificationServiceEnabled = false;
                    SharedViewModel.IsOvrToolkitNotificationServiceEnabled = false;
                    break;
                case NotificationService.XSOverlay:
                    SharedViewModel.IsXsOverlayNotificationServiceEnabled = true;
                    SharedViewModel.IsSteamVrNotificationServiceEnabled = false;
                    SharedViewModel.IsOvrToolkitNotificationServiceEnabled = false;
                    break;
                case NotificationService.OVRToolkit:
                    SharedViewModel.IsOvrToolkitNotificationServiceEnabled = true;
                    SharedViewModel.IsXsOverlayNotificationServiceEnabled = false;
                    SharedViewModel.IsSteamVrNotificationServiceEnabled = false;
                    break;
                default:
                    SharedViewModel.IsXsOverlayNotificationServiceEnabled = false;
                    SharedViewModel.IsSteamVrNotificationServiceEnabled = false;
                    SharedViewModel.IsOvrToolkitNotificationServiceEnabled = false;
                    break;
            }

            _isNotificationServiceReady = SharedViewModel.IsSteamVrNotificationServiceEnabled || SharedViewModel.IsXsOverlayNotificationServiceEnabled;
        }

    }
}
