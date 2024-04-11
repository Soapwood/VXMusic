using System;
using System.Windows;
using System.Windows.Threading;
using VXMusic.Overlay;
using VXMusicDesktop.Core;
using VXMusicDesktop.Theme;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        private static DispatcherTimer _checkOverlayHeartbeatMonitor;
        private static readonly int _checkOverlayHeartbeatInterval = 8;
        
        /*
         *  Menu Navigation
         */

        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand NotificationsViewCommand { get; set; }
        public RelayCommand RecognitionViewCommand { get; set; }
        public RelayCommand ConnectionsViewCommand { get; set; }
        public RelayCommand OverlayViewCommand { get; set; }
        public RelayCommand AboutViewCommand { get; set; }
        public RelayCommand LaunchVXMusicOverlay { get; set; }

        public static SharedViewModel SharedVM { get; set; }
        public HomeViewModel HomeVM { get; set; }
        public NotificationsViewModel NotificationsVM { get; set; }
        public RecognitionViewModel RecognitionVM { get; set; }

        public ConnectionsViewModel ConnectionsVM { get; set; }
        public OverlayViewModel OverlayVm { get; set; }
        public AboutViewModel AboutVM { get; set; }


        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set { 
                _currentView = value;
                OnPropertyChanged();
                ColourSchemeManager.RaiseMenuOptionChanged();
            }
        }

        ///*
        // *  Main menu inputs 
        // */
        public MainViewModel()
        {
            SharedVM = new SharedViewModel();
            
            HomeVM = new HomeViewModel(SharedVM);
            NotificationsVM = new NotificationsViewModel(SharedVM);
            RecognitionVM = new RecognitionViewModel(SharedVM);
            ConnectionsVM = new ConnectionsViewModel(SharedVM);
            OverlayVm = new OverlayViewModel();
            AboutVM = new AboutViewModel();
            CurrentView = HomeVM;

            VXMusicOverlayInterface.SharedViewModel = SharedVM;

            HomeViewCommand = new RelayCommand(o =>
            {
                CurrentView = HomeVM;
            });

            NotificationsViewCommand = new RelayCommand(o =>
            {
                CurrentView = NotificationsVM;
            });

            RecognitionViewCommand = new RelayCommand(o =>
            {
                CurrentView = RecognitionVM;
            });

            ConnectionsViewCommand = new RelayCommand(o =>
            {
                CurrentView = ConnectionsVM;
            });

            OverlayViewCommand = new RelayCommand(o =>
            {
                CurrentView = OverlayVm;
            });

            AboutViewCommand = new RelayCommand(o =>
            {
                CurrentView = AboutVM;
            });
            
            LaunchVXMusicOverlay = new RelayCommand(o =>
            {
                App.VXMOverlayProcessId =
                    VXMusicOverlayInterface.LaunchVXMOverlayRuntime(VXMusicSession.OverlaySettings.RuntimePath);
                InitialiseOverlayHeartbeatMonitor();
            });
        }
        
        public static void InitialiseOverlayHeartbeatMonitor()
        {
            _checkOverlayHeartbeatMonitor = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_checkOverlayHeartbeatInterval)
            };
            _checkOverlayHeartbeatMonitor.Tick += CheckOverlayRunningState;
            _checkOverlayHeartbeatMonitor.Start();
        }
        
        private static void CheckOverlayRunningState(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SharedVM.IsOverlayRunning = VXMusicOverlayInterface.HasNewHeartbeatMessage;
                VXMusicOverlayInterface.HasNewHeartbeatMessage = false;
            });
        }
    }
}
