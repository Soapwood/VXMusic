using System;
using System.Threading;
using System.Windows.Input;
using VXMusic;
using VXMusicDesktop.Core;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic.API;
using VXMusic.FileWriter;
using System.Threading.Tasks;
using VXMusic.Spotify.Authentication;

namespace VXMusicDesktop.MVVM.ViewModel
{

    internal class RecognitionViewModel : INotifyPropertyChanged
    {
        public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();

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
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.Shazam);
            RecognitionSettings.SetRecognitionApiInSettings(RecognitionApi.Shazam);
            ProcessRecognitionApiState();
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.AudD);
            RecognitionSettings.SetRecognitionApiInSettings(RecognitionApi.AudD);
            ProcessRecognitionApiState();
        }

        private async void PerformListenButtonClick(object commandParameter)
        {
            // TODO Two recognitions can run at the same time, add check to disable button if it's already running
            Logger.LogInformation("Running Recognition Flow from Desktop Client Trigger.");
            VXMusicActions.PerformRecognitionFlow();
        }
    }


}
