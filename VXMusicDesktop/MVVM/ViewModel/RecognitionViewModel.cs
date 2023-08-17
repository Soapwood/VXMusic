using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusic;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;
using VXMusicDesktop.Core;
using VXMusicDesktop;
using System.ComponentModel;

namespace VXMusicDesktop.MVVM.ViewModel
{

    internal class RecognitionViewModel : INotifyPropertyChanged
    {
        private RelayCommand shazamButtonClick;
        private RelayCommand audDButtonClick;
        private RelayCommand listenButtonClick;

        private RelayCommand recognitionViewLoaded;

        public ICommand ShazamButtonClick => shazamButtonClick ??= new RelayCommand(PerformShazamButtonClick);
        public ICommand AudDButtonClick => audDButtonClick ??= new RelayCommand(PerformAudDButtonClick);
        public ICommand ListenButtonClick => listenButtonClick ??= new RelayCommand(PerformListenButtonClick);
        public ICommand RecognitionViewLoaded => recognitionViewLoaded ??= new RelayCommand(OnRecognitionViewLoaded);

        // Concurrency fields
        public static String CurrentRecognitionApi;
        public static bool ShazamApi = true;

        public static String AudDButtonText = "Donkey fungus";

        // // //
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private bool _isShazamApiEnabled;
        private bool _isAudDApiEnabled;

        private bool _isRecognitionReady = true;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsShazamApiEnabled
        {
            get { return _isShazamApiEnabled; }
            set
            {
                _isShazamApiEnabled = value;
                OnPropertyChanged(nameof(IsShazamApiEnabled));
            }
        }

        public bool IsAudDApiEnabled
        {
            get { return _isAudDApiEnabled; }
            set
            {
                _isAudDApiEnabled = value;
                OnPropertyChanged(nameof(IsAudDApiEnabled)); 
            }
        }

        public bool IsRecognitionReady
        {
            get { return _isRecognitionReady; }
            set
            {
                _isRecognitionReady = value;
                OnPropertyChanged(nameof(IsRecognitionReady)); 
            }
        }

        private void OnRecognitionViewLoaded(object commandParameter)
        {
            ProcessRecognitionApiState();
        }

        private void ProcessRecognitionApiState()
        {
            switch (App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi)
            {
                case RecognitionApi.Shazam:
                    IsShazamApiEnabled = true;
                    IsAudDApiEnabled = false;
                    break;
                case RecognitionApi.AudD:
                    IsAudDApiEnabled = true;
                    IsShazamApiEnabled = false;
                    break;
                default:
                    IsShazamApiEnabled = false;
                    IsAudDApiEnabled = false;
                    break;
            }

            _isRecognitionReady = IsShazamApiEnabled || IsAudDApiEnabled;
        }

        private void PerformShazamButtonClick(object commandParameter)
        {
            
            App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi = RecognitionApi.Shazam;
            App.VXMusicSession.RecognitionClient = new ShazamClient();

            ProcessRecognitionApiState();
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi = RecognitionApi.AudD;
            App.VXMusicSession.RecognitionClient = new AudDClient();

            ProcessRecognitionApiState();
        }

        private async void PerformListenButtonClick(object commandParameter)
        {
            // TODO Two recognitions can run at the same time, add check to disable button if it's already running
            VXMusicAPI.RunRecording();
            //var result = //await VXMusicAPI.RunRecognition();
            var result = await App.VXMusicSession.RecognitionClient.RunRecognition();

            if (result.Status == Status.Error)
            {
                App.VXMusicSession.NotificationClient.SendNotification("Recognition failed! Oh jaysus", "", 5);
                Trace.WriteLine("Recognition failed! Oh jaysus");
                //Environment.Exit(0);
            }
            else if (result.Status == Status.NoMatches || result.Result == null)
            {
                App.VXMusicSession.NotificationClient.SendNotification("Oops, couldn't get that.", "Tech Tip: Have you tried turning up your World Volume?", 5);
                Trace.WriteLine("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
            }
            else
            {
                App.VXMusicSession.NotificationClient.SendNotification($"{result.Result.Artist} - {result.Result.Title}", $"{result.Result.Album} ({result.Result.ReleaseDate})", 8);
                Trace.WriteLine($"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
            }

            if(result.Result != null && App.VXMusicSession.ConnectionsSettings.IsSpotifyConnected)
                VXMusicAPI.ReportTrackToSpotifyPlaylist(result);

            if (result.Result != null)
            {
                VXMusicAPI.Scrobble(result.Result.Artist, result.Result.Album, result.Result.Title);
            }
        }
    }


}
