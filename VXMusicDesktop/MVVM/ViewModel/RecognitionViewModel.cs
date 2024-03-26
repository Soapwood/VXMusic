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
        
        private RelayCommand shazamByoApiRadioButtonChecked;
        private RelayCommand shazamByoApiRadioButtonUnchecked;
        private RelayCommand audDByoApiRadioButtonChecked;
        private RelayCommand audDByoApiRadioButtonUnchecked;

        private RelayCommand listenButtonClick;

        private RelayCommand recognitionViewLoaded;

        private string _shazamByoApiToken = "ApiToken";
        private string _audDByoApiToken = "ApiToken";

        public ICommand ShazamButtonClick => shazamButtonClick ??= new RelayCommand(PerformShazamButtonClick);
        public ICommand AudDButtonClick => audDButtonClick ??= new RelayCommand(PerformAudDButtonClick);
        public ICommand ShazamByoApiCheckButtonChecked => shazamByoApiRadioButtonChecked ??= new RelayCommand(PerformEnableShazamByoApiChecked);
        public ICommand ShazamByoApiCheckButtonUnchecked => shazamByoApiRadioButtonUnchecked ??= new RelayCommand(PerformEnableShazamByoApiUnchecked);
        public ICommand AudDByoApiCheckButtonChecked => audDByoApiRadioButtonChecked ??= new RelayCommand(PerformEnableAudDByoApiChecked);
        public ICommand AudDByoApiCheckButtonUnchecked => audDByoApiRadioButtonUnchecked ??= new RelayCommand(PerformEnableAudDByoApiUnchecked);
        public ICommand ListenButtonClick => listenButtonClick ??= new RelayCommand(PerformListenButtonClick);
        public ICommand RecognitionViewLoaded => recognitionViewLoaded ??= new RelayCommand(OnRecognitionViewLoaded);

        // Concurrency fields
        public static String CurrentRecognitionApi;
        public static bool ShazamApi = true;

        // // //
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private bool _isShazamApiEnabled;
        private bool _isShazamByoApiEnabled;
        private bool _isAudDApiEnabled;
        private bool _isAudDByoApiEnabled;

        private bool _isRecognitionReady = true;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public string ShazamByoApiToken
        {
            get { return _shazamByoApiToken; }
            set
            {
                if (_shazamByoApiToken != value)
                {
                    _shazamByoApiToken = value;
                    OnPropertyChanged(nameof(ShazamByoApiToken));
                }
            }
        }
        
        public string AudDByoApiToken
        {
            get { return _audDByoApiToken; }
            set
            {
                if (_audDByoApiToken != value)
                {
                    _audDByoApiToken = value;
                    OnPropertyChanged(nameof(AudDByoApiToken));
                }
            }
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

        public bool IsShazamByoApiEnabled
        {
            get { return _isShazamByoApiEnabled; }
            set
            {
                _isShazamByoApiEnabled = value;
                OnPropertyChanged(nameof(IsShazamByoApiEnabled));
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

        public bool IsAudDByoApiEnabled
        {
            get { return _isAudDByoApiEnabled; }
            set
            {
                _isAudDByoApiEnabled = value;
                OnPropertyChanged(nameof(IsAudDByoApiEnabled));
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
            
            Logger.LogInformation($"Recognition API set to {App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi}");
        }
        
        public void PerformSaveAndTestShazamByoApi()
        {
            if (String.IsNullOrEmpty(_shazamByoApiToken))
                return;
            
            VXUserSettings.Recognition.SetShazamByoApiKey(_shazamByoApiToken);
            VXUserSettings.Recognition.SetIsShazamByoApiEnabled(true);
            
            VXMusicSession.RecognitionClient.SetByoApiKeyAndTest(_shazamByoApiToken);
        }
        

        private void PerformShazamButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.Shazam);
            VXUserSettings.Recognition.SetRecognitionApi(RecognitionApi.Shazam);
            ProcessRecognitionApiState();
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.AudD);
            VXUserSettings.Recognition.SetRecognitionApi(RecognitionApi.AudD);
            ProcessRecognitionApiState();
        }

        private void PerformEnableShazamByoApiChecked(object commandParameter)
        {
            IsShazamByoApiEnabled = true;
            VXUserSettings.Recognition.SetIsShazamByoApiEnabled(IsShazamByoApiEnabled);
        }
        
        private void PerformEnableShazamByoApiUnchecked(object commandParameter)
        {
            IsShazamByoApiEnabled = false;
            VXUserSettings.Recognition.SetIsShazamByoApiEnabled(IsShazamByoApiEnabled);
        }

        private void PerformEnableAudDByoApiChecked(object commandParameter)
        {
            IsAudDByoApiEnabled = true;
            VXUserSettings.Recognition.SetIsAudDByoApiEnabled(IsAudDByoApiEnabled);
        }
        
        private void PerformEnableAudDByoApiUnchecked(object commandParameter)
        {
            IsAudDByoApiEnabled = false;
            VXUserSettings.Recognition.SetIsAudDByoApiEnabled(IsAudDByoApiEnabled);
        }
        
        private void PerformSaveAndTestAudDByoApiUnchecked(object commandParameter)
        {
            if (String.IsNullOrEmpty(_audDByoApiToken))
                return;

            
            //App.VXMusicSession.SetRecognitionClient(RecognitionApi.AudD);
            //RecognitionSettings.SetRecognitionApiInSettings(RecognitionApi.AudD);
            //ProcessRecognitionApiState();
        }

        private async void PerformListenButtonClick(object commandParameter)
        {
            // TODO Two recognitions can run at the same time, add check to disable button if it's already running
            Logger.LogInformation("Running Recognition Flow from Desktop Client Trigger.");
            VXMusicActions.PerformRecognitionFlow();
        }
    }


}
