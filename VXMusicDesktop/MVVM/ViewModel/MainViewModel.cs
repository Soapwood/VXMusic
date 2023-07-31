using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand NotificationsViewCommand { get; set; }
        public RelayCommand RecognitionViewCommand { get; set; }
        public RelayCommand SpotifyViewCommand { get; set; }
        public RelayCommand BindingsViewCommand { get; set; }
        public RelayCommand AboutViewCommand { get; set; }

        public HomeViewModel HomeVM { get; set; }
        public NotificationsViewModel NotificationsVM { get; set; }
        public RecognitionViewModel RecognitionVM { get; set; }

        public SpotifyViewModel SpotifyVM { get; set; }
        public BindingsViewModel BindingsVM { get; set; }
        public AboutViewModel AboutVM { get; set; }


        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set { 
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel()
        { 
            HomeVM = new HomeViewModel();
            NotificationsVM = new NotificationsViewModel();
            RecognitionVM = new RecognitionViewModel();
            SpotifyVM = new SpotifyViewModel();
            BindingsVM = new BindingsViewModel();
            AboutVM = new AboutViewModel();
            CurrentView = HomeVM;

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

            SpotifyViewCommand = new RelayCommand(o =>
            {
                CurrentView = SpotifyVM;
            });

            BindingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = BindingsVM;
            });

            AboutViewCommand = new RelayCommand(o =>
            {
                CurrentView = AboutVM;
            });
        }
    }
}
