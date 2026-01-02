using VXMusic;
using VXMusic.Overlay;
using VXMusicDesktop.Core;
using VXMusicDesktop.Theme;
using VXMusicDesktop.Update;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class MainViewModel : ObservableObject
    {
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
        public RelayCommand LaunchVXUpdater { get; set; }

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
            VXMusicSession.SharedViewModel = SharedVM;

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
                if (VXMusicOverlayInterface.IsOverlayConnected)
                {
                    App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Info,
                        "Overlay is already running!","", 3);
                }
                else
                {
                    App.VXMOverlayProcess =
                        VXMusicOverlayInterface.LaunchVXMOverlayRuntime(App.VXMusicSession.OverlaySettings.RuntimePath);
                }
            });
            
            LaunchVXUpdater = new RelayCommand(o =>
            {
                App.VXMusicSession.VxMusicUpdate.LaunchVxMusicUpdater();
            });
        }
    }
}
