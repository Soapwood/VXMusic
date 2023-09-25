using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using VXMusic;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
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
            }
        }

        ///*
        // *  Main menu inputs 
        // */
        //private RelayCommand themeDropdown;

        //public ICommand ThemeDropdown => themeDropdown ??= new RelayCommand(ThemeDropdownSelection);

        public MainViewModel()
        { 
            HomeVM = new HomeViewModel();
            NotificationsVM = new NotificationsViewModel();
            RecognitionVM = new RecognitionViewModel();
            ConnectionsVM = new ConnectionsViewModel();
            OverlayVm = new OverlayViewModel();
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
        }
    }
}
