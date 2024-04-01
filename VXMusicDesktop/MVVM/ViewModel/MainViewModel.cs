﻿using System;
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
using VXMusicDesktop.Theme;

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

        public SharedViewModel SharedVM { get; }
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
            NotificationsVM = new NotificationsViewModel();
            RecognitionVM = new RecognitionViewModel(SharedVM);
            ConnectionsVM = new ConnectionsViewModel(SharedVM);
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
