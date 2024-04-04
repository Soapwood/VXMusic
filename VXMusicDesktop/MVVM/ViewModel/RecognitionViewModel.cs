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
    public class RecognitionViewModel : INotifyPropertyChanged
    {
        public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();

        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }

        private RelayCommand shazamButtonClick;
        private RelayCommand audDButtonClick;
        
        private RelayCommand shazamByoApiRadioButtonChecked;
        private RelayCommand shazamByoApiRadioButtonUnchecked;
        private RelayCommand audDByoApiRadioButtonChecked;
        private RelayCommand audDByoApiRadioButtonUnchecked;

        private RelayCommand listenButtonClick;

        private RelayCommand recognitionViewLoaded;

        private string _shazamByoApiToken = "";
        private string _audDByoApiToken = "";

        // Bindings for mapping interaction commands to Bindings to be used in Views
        // i.e. RecognitionView will reference the Binding ShazamButtonClick to run PerformShazamButtonClick
        public ICommand ShazamButtonClick => shazamButtonClick ??= new RelayCommand(PerformShazamButtonClick);
         public ICommand AudDButtonClick => audDButtonClick ??= new RelayCommand(PerformAudDButtonClick);
         public ICommand ShazamByoApiCheckButtonChecked => shazamByoApiRadioButtonChecked ??= new RelayCommand(PerformEnableShazamByoApiChecked);
         public ICommand ShazamByoApiCheckButtonUnchecked => shazamByoApiRadioButtonUnchecked ??= new RelayCommand(PerformEnableShazamByoApiUnchecked);
         public ICommand AudDByoApiCheckButtonChecked => audDByoApiRadioButtonChecked ??= new RelayCommand(PerformEnableAudDByoApiChecked);
         public ICommand AudDByoApiCheckButtonUnchecked => audDByoApiRadioButtonUnchecked ??= new RelayCommand(PerformEnableAudDByoApiUnchecked);
         public ICommand ListenButtonClick => listenButtonClick ??= new RelayCommand(PerformListenButtonClick);
         public ICommand RecognitionViewLoaded => recognitionViewLoaded ??= new RelayCommand(OnRecognitionViewLoaded);

        // // //
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool HasBeenInitialised;
        
        private bool _isShazamApiEnabled;
        private bool _isShazamByoApiEnabled;
        //private bool _isShazamApiConnected;
        private bool _isAudDApiEnabled;
        private bool _isAudDByoApiEnabled;
       // private bool _isAudDApiConnected;

        private bool _isRecognitionReady = true;

        public RecognitionViewModel()
        {
            // Get default state of RecognitionViewModel on UI Startup
            Logger.LogDebug("Creating RecognitionViewModel");

            if (!HasBeenInitialised)
                Initialise();
        }

        public RecognitionViewModel(SharedViewModel sharedViewModel)
        {
            SharedViewModel = sharedViewModel;
            Initialise();
        }

        public void Initialise()
        {
            _isShazamApiEnabled = App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi == RecognitionApi.Shazam;
            _isShazamByoApiEnabled = App.VXMusicSession.RecognitionSettings.ShazamSettings.IsByoApiEnabled;
            _shazamByoApiToken = App.VXMusicSession.RecognitionSettings.ShazamSettings.ByoApiKey;

            _isAudDApiEnabled = App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi == RecognitionApi.AudD;         
            _isAudDByoApiEnabled = App.VXMusicSession.RecognitionSettings.AudDSettings.IsByoApiEnabled;
            _audDByoApiToken = App.VXMusicSession.RecognitionSettings.AudDSettings.ByoApiKey;
            ProcessRecognitionApiState();

            HasBeenInitialised = true;
            // TODO Do we actually want to do this here? It may be better to trigger this in the HomeView
            CheckIfCurrentApiIsConnected();
        }
        
        private void OnRecognitionViewLoaded(object commandParameter)
        {
            ProcessRecognitionApiState();
            //CheckIfCurrentApiIsConnected();
        }

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

        private void ProcessRecognitionApiState()
        {
            switch (App.VXMusicSession.RecognitionSettings?.CurrentRecognitionApi)
            {
                case RecognitionApi.Shazam:
                    IsShazamApiEnabled = true;
                    IsShazamByoApiEnabled = App.VXMusicSession.RecognitionSettings.ShazamSettings.IsByoApiEnabled && 
                                            !String.IsNullOrEmpty(App.VXMusicSession.RecognitionSettings.ShazamSettings.ByoApiKey);
                    
                    IsAudDApiEnabled = false;
                    IsAudDByoApiEnabled = false; //App.VXMusicSession.RecognitionSettings.AudDSettings.IsByoApiEnabled && 
                                          //!String.IsNullOrEmpty(App.VXMusicSession.RecognitionSettings.AudDSettings.ByoApiKey);
                    break;
                case RecognitionApi.AudD:
                    IsAudDApiEnabled = true;
                    IsAudDByoApiEnabled = App.VXMusicSession.RecognitionSettings.AudDSettings.IsByoApiEnabled && 
                                          !String.IsNullOrEmpty(App.VXMusicSession.RecognitionSettings.AudDSettings.ByoApiKey);

                    IsShazamApiEnabled = false;
                    IsShazamByoApiEnabled = false; //App.VXMusicSession.RecognitionSettings.ShazamSettings.IsByoApiEnabled && 
                                            //!String.IsNullOrEmpty(App.VXMusicSession.RecognitionSettings.ShazamSettings.ByoApiKey);
                    break;
                default:
                    IsShazamApiEnabled = false;
                    IsAudDApiEnabled = false;
                    
                    IsShazamByoApiEnabled = App.VXMusicSession.RecognitionSettings.ShazamSettings.IsByoApiEnabled;
                    IsAudDByoApiEnabled = App.VXMusicSession.RecognitionSettings.AudDSettings.IsByoApiEnabled;
                    break;
            }

            _isRecognitionReady = IsShazamApiEnabled || IsAudDApiEnabled;
            
            Logger.LogInformation($"Recognition API set to {App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi}");
        }
        
        public async Task CheckIfCurrentApiIsConnected()
        {
            switch (App.VXMusicSession.RecognitionSettings?.CurrentRecognitionApi)
            {
                case RecognitionApi.Shazam:
                    SharedViewModel.IsShazamApiConnected = await VXMusicSession.RecognitionClient.TestApiConnection();
                    break;
                case RecognitionApi.AudD:
                    SharedViewModel.IsAudDApiConnected = await VXMusicSession.RecognitionClient.TestApiConnection();
                    break;
                default:
                    SharedViewModel.IsShazamApiConnected = false;
                    SharedViewModel.IsAudDApiConnected = false;
                    break;
            }

            _isRecognitionReady = IsShazamApiEnabled || IsAudDApiEnabled;
            
            Logger.LogDebug($"Current Recognition API is {App.VXMusicSession.RecognitionSettings?.CurrentRecognitionApi}");
        }
        
        /// ////////////////////////////////////
        /// UI Interactive Update Functions
        /// ////////////////////////////////////

        private async void PerformListenButtonClick(object commandParameter)
        {
            // TODO Two recognitions can run at the same time, add check to disable button if it's already running
            Logger.LogInformation("Running Recognition Flow from Desktop Client Trigger.");
            VXMusicActions.PerformRecognitionFlow();
        }
        
        public async Task<bool> PerformSaveAndTestShazamByoApi()
        {
            if (String.IsNullOrEmpty(_shazamByoApiToken))
                return false;
            
            VXUserSettings.Recognition.SetShazamByoApiKey(_shazamByoApiToken);
            VXUserSettings.Recognition.SetIsShazamByoApiEnabled(true);
            
            SharedViewModel.IsShazamApiConnected = await VXMusicSession.RecognitionClient.SetByoApiKeyAndTest(_shazamByoApiToken);

            return SharedViewModel.IsShazamApiConnected;
        }
        
        public async Task<bool> PerformSaveAndTestAudDByoApi()
        {
            if (String.IsNullOrEmpty(_audDByoApiToken))
                return false;
            
            VXUserSettings.Recognition.SetAudDByoApiKey(_audDByoApiToken);
            VXUserSettings.Recognition.SetIsAudDByoApiEnabled(true);
            
            SharedViewModel.IsAudDApiConnected = await VXMusicSession.RecognitionClient.SetByoApiKeyAndTest(_audDByoApiToken);

            return SharedViewModel.IsAudDApiConnected;
        }
        
        public async Task<bool> SetShazamApiKeyToDefaultAndTest()
        {
            VXMusicSession.RecognitionClient.SetDefaultApiKey();
            SharedViewModel.IsShazamApiConnected = await VXMusicSession.RecognitionClient.TestApiConnection();

            return SharedViewModel.IsShazamApiConnected;
        }
        
        public async Task<bool> SetAudDApiKeyToDefaultAndTest()
        {
            VXMusicSession.RecognitionClient.SetDefaultApiKey();
            SharedViewModel.IsAudDApiConnected = await VXMusicSession.RecognitionClient.TestApiConnection();

            return SharedViewModel.IsAudDApiConnected;
        }

        private void PerformShazamButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.Shazam);
            VXUserSettings.Recognition.SetRecognitionApi(RecognitionApi.Shazam);
            
            SharedViewModel.IsAudDApiConnected = false;
            IsAudDApiEnabled = false;
            IsAudDByoApiEnabled = false;
            
            ProcessRecognitionApiState();
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.AudD);
            VXUserSettings.Recognition.SetRecognitionApi(RecognitionApi.AudD);
            
            SharedViewModel.IsShazamApiConnected = false;
            IsShazamApiEnabled = false;
            IsShazamByoApiEnabled = false;
            
            ProcessRecognitionApiState();
        }

        private void PerformEnableShazamByoApiChecked(object commandParameter)
        {
            IsShazamByoApiEnabled = true;
            VXUserSettings.Recognition.SetIsShazamByoApiEnabled(IsShazamByoApiEnabled);

            PerformSaveAndTestShazamByoApi();
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
    }
}
