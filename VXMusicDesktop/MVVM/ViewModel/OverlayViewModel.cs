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

        private RelayCommand steamVREnableButtonClick;
        private bool _isSteamVRNotificationServiceEnabled;
        public ICommand LeftHandTrackedDeviceOverlay => steamVREnableButtonClick ??= new RelayCommand(SetLeftHandTrackedDeviceOverlay);

        private RelayCommand xsOverlayEnableButtonClick;
        private bool _isXSOverlayNotificationServiceEnabled;
        public ICommand RightHandTrackedDeviceOverlay => xsOverlayEnableButtonClick ??= new RelayCommand(SetRightHandTrackedDeviceOverlay);

        private void SetLeftHandTrackedDeviceOverlay(object commandParameter)
        {
            //App.VXMusicSession.SetNotificationService(NotificationService.SteamVR);
            //NotificationSettings.SetNotificationServiceInSettings(NotificationService.SteamVR);
            //ProcessNotificationServiceState();
            //VXMusicSession.RaiseSteamVrNotificationEnabled();
            VXMusicSession.VXMusicOverlay.SetLeftOverlayTrackedDevice();

        }

        private void SetRightHandTrackedDeviceOverlay(object commandParameter)
        {
            //App.VXMusicSession.SetNotificationService(NotificationService.XSOverlay);
            //NotificationSettings.SetNotificationServiceInSettings(NotificationService.XSOverlay);
            //ProcessNotificationServiceState();
            //VXMusicSession.RaiseXsOverlayNotificationEnabled();
            VXMusicSession.VXMusicOverlay.SetRightHandOverlayTrackedDevice();

        }
    }
}
