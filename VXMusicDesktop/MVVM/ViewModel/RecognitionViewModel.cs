using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Devices;
using VXMusic;
using VXMusic.Audio.Device;
using VXMusic.Audio.Recording;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class RecognitionViewModel : INotifyPropertyChanged
    {
        public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<RecognitionViewModel>>();

        private static readonly string LogsOutputPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VXMusic");

        #region Private Properties
        
        public event PropertyChangedEventHandler? PropertyChanged;
        public bool HasBeenInitialised;

        private List<AudioDevice> _availableAudioDevices;
        private AudioDevice _selectedAudioDevice;
        
        private string _shazamByoApiToken = "";
        private string _audDByoApiToken = "";
        private bool _isShazamApiEnabled;
        private bool _isShazamByoApiEnabled;
        private bool _isAudDApiEnabled;
        private bool _isAudDByoApiEnabled;
        private bool _shouldShazamByoApiPlaceholderBeShown;
        private bool _shouldAudDByoApiPlaceholderBeShown;
        private bool _isRecognitionReady = true;
        
        #endregion
        
        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }

        #region Commands

        public ICommand ShazamButtonClick => shazamButtonClick ??= new RelayCommand(PerformShazamButtonClick);
        public ICommand AudDButtonClick => audDButtonClick ??= new RelayCommand(PerformAudDButtonClick);

        public ICommand ShazamByoApiCheckButtonChecked =>
            shazamByoApiRadioButtonChecked ??= new RelayCommand(PerformEnableShazamByoApiChecked);

        public ICommand ShazamByoApiCheckButtonUnchecked => shazamByoApiRadioButtonUnchecked ??=
            new RelayCommand(PerformEnableShazamByoApiUnchecked);

        public ICommand AudDByoApiCheckButtonChecked =>
            audDByoApiRadioButtonChecked ??= new RelayCommand(PerformEnableAudDByoApiChecked);

        public ICommand AudDByoApiCheckButtonUnchecked =>
            audDByoApiRadioButtonUnchecked ??= new RelayCommand(PerformEnableAudDByoApiUnchecked);

        public ICommand ListenButtonClick => listenButtonClick ??= new RelayCommand(PerformListenButtonClick);
        public ICommand RecognitionViewLoaded => recognitionViewLoaded ??= new RelayCommand(OnRecognitionViewLoaded);

        public ICommand PasswordHintTextVisibilityToggle =>
            recognitionViewLoaded ??= new RelayCommand(OnRecognitionOptionChanged);

        public ICommand OpenFolderCommand => openFolderClick ??= new RelayCommand(OpenFolder);

        private RelayCommand shazamButtonClick;
        private RelayCommand audDButtonClick;
        private RelayCommand shazamByoApiRadioButtonChecked;
        private RelayCommand shazamByoApiRadioButtonUnchecked;
        private RelayCommand audDByoApiRadioButtonChecked;
        private RelayCommand audDByoApiRadioButtonUnchecked;
        private RelayCommand listenButtonClick;
        private RelayCommand recognitionViewLoaded;
        private RelayCommand openFolderClick;
        private RelayCommand selectAudioDeviceClick;

        #endregion

        #region State Properties

        public List<AudioDevice> AvailableAudioDevices
        {
            get => _availableAudioDevices;
            set
            {
                _availableAudioDevices = value;
                OnPropertyChanged(nameof(AvailableAudioDevices));
            }
        }

        public AudioDevice SelectedAudioDevice
        {
            get => _selectedAudioDevice;
            set
            {
                if (_selectedAudioDevice != value)
                {
                    _selectedAudioDevice = value;
                    OnPropertyChanged(nameof(SelectedAudioDevice));
                    OnSelectedDeviceChanged();
                }
            }
        }

        public string ShazamByoApiToken
        {
            get => _shazamByoApiToken;
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
            get => _audDByoApiToken;
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
            get => _isShazamApiEnabled;
            set
            {
                _isShazamApiEnabled = value;
                ShouldAudDByoApiPlaceholderBeShown = false;
                OnPropertyChanged(nameof(IsShazamApiEnabled));
            }
        }

        public bool IsShazamByoApiEnabled
        {
            get => _isShazamByoApiEnabled;
            set
            {
                _isShazamByoApiEnabled = value;
                OnPropertyChanged(nameof(IsShazamByoApiEnabled));
            }
        }

        public bool ShouldShazamByoApiPlaceholderBeShown
        {
            get => _isShazamApiEnabled && !string.IsNullOrEmpty(_shazamByoApiToken);
            set
            {
                _shouldShazamByoApiPlaceholderBeShown = value;
                OnPropertyChanged(nameof(ShouldShazamByoApiPlaceholderBeShown));
            }
        }

        public bool IsAudDApiEnabled
        {
            get => _isAudDApiEnabled;
            set
            {
                _isAudDApiEnabled = value;
                ShouldShazamByoApiPlaceholderBeShown = false;
                OnPropertyChanged(nameof(IsAudDApiEnabled));
            }
        }

        public bool IsRecognitionReady
        {
            get => _isRecognitionReady;
            set
            {
                _isRecognitionReady = value;
                OnPropertyChanged(nameof(IsRecognitionReady));
            }
        }

        public bool ShouldAudDByoApiPlaceholderBeShown
        {
            get { return _isAudDApiEnabled && !string.IsNullOrEmpty(_audDByoApiToken); }
            set
            {
                _shouldAudDByoApiPlaceholderBeShown = value;
                OnPropertyChanged(nameof(ShouldAudDByoApiPlaceholderBeShown));
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

        #endregion

        public RecognitionViewModel(SharedViewModel sharedViewModel)
        {
            SharedViewModel = sharedViewModel;
            // Get default state of RecognitionViewModel on UI Startup
            Logger.LogDebug("Creating RecognitionViewModel");

            if (!HasBeenInitialised)
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

            _shouldShazamByoApiPlaceholderBeShown = _isShazamApiEnabled && !string.IsNullOrEmpty(_shazamByoApiToken);
            _shouldAudDByoApiPlaceholderBeShown = _isAudDApiEnabled && !string.IsNullOrEmpty(_audDByoApiToken);

            HasBeenInitialised = true;
            // TODO Do we actually want to do this here? It may be better to trigger this in the HomeView
            CheckIfCurrentApiIsConnected();

            AvailableAudioDevices = WindowsAudioDeviceListener.AvailableAudioDevices;

            SelectedAudioDevice = DetermineSelectedAudioDeviceFromUserSettings(); 
        }
        
        #region Events
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnRecognitionViewLoaded(object commandParameter)
        {
            ProcessRecognitionApiState();
        }

        private void OnRecognitionOptionChanged(object commandParameter)
        {
            ShouldShazamByoApiPlaceholderBeShown = !ShouldShazamByoApiPlaceholderBeShown;
            ShouldAudDByoApiPlaceholderBeShown = !ShouldAudDByoApiPlaceholderBeShown;

            CheckIfCurrentApiIsConnected();
        }
        
        private void OnSelectedDeviceChanged()
        {
            Logger.LogInformation($"Listening Audio Device changed to {SelectedAudioDevice}");
            
            WindowsAudioDeviceListener.SelectedAudioDevice = SelectedAudioDevice;
            VXUserSettings.Recognition.SetSelectedAudioDevice(SelectedAudioDevice.DeviceFriendlyName);
        }
        
        #endregion

        private void ProcessRecognitionApiState()
        {
            switch (App.VXMusicSession.RecognitionSettings?.CurrentRecognitionApi)
            {
                case RecognitionApi.Shazam:
                    IsShazamApiEnabled = true;
                    IsShazamByoApiEnabled = App.VXMusicSession.RecognitionSettings.ShazamSettings.IsByoApiEnabled;
                    //&& !String.IsNullOrEmpty(App.VXMusicSession.RecognitionSettings.ShazamSettings.ByoApiKey);

                    IsAudDApiEnabled = false;
                    //IsAudDByoApiEnabled = false;
                    break;
                case RecognitionApi.AudD:
                    IsAudDApiEnabled = true;
                    IsAudDByoApiEnabled = App.VXMusicSession.RecognitionSettings.AudDSettings.IsByoApiEnabled;
                    //&& !String.IsNullOrEmpty(App.VXMusicSession.RecognitionSettings.AudDSettings.ByoApiKey);

                    IsShazamApiEnabled = false;
                    //IsShazamByoApiEnabled = false;
                    break;
                default:
                    IsShazamApiEnabled = false;
                    IsAudDApiEnabled = false;

                    IsShazamByoApiEnabled = App.VXMusicSession.RecognitionSettings.ShazamSettings.IsByoApiEnabled;
                    IsAudDByoApiEnabled = App.VXMusicSession.RecognitionSettings.AudDSettings.IsByoApiEnabled;
                    break;
            }

            _isRecognitionReady = IsShazamApiEnabled || IsAudDApiEnabled;

            Logger.LogInformation(
                $"Recognition API set to {App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi}");
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

            Logger.LogDebug(
                $"Current Recognition API is {App.VXMusicSession.RecognitionSettings?.CurrentRecognitionApi}");
        }

        private AudioDevice DetermineSelectedAudioDeviceFromUserSettings()
        {
            var userSelectedAudioDevice = VXUserSettings.Recognition.GetSelectedAudioDevice();
            
            Logger.LogTrace($"Found User Set AudioDevice: {userSelectedAudioDevice}");

            if (userSelectedAudioDevice == AudioDevice.Default.DeviceFriendlyName)
                return AudioDevice.Default;

            if (string.IsNullOrEmpty(userSelectedAudioDevice))
            {
                Logger.LogWarning("Audio Device was not set in user settings. Using Default.");
                return AudioDevice.Default;
            }
                
            Logger.LogInformation($"Setting Audio Device found in User Settings: {userSelectedAudioDevice}");
            return new AudioDevice(){DeviceFriendlyName = userSelectedAudioDevice, IsDefaultAudioDevice = false};
        }

        /// ////////////////////////////////////
        /// UI Interactive Update Functions
        /// ////////////////////////////////////
        private async void PerformListenButtonClick(object commandParameter)
        {
            if (!SharedViewModel.IsRecognitionRunning)
            {
                Logger.LogInformation("Running Recognition Flow from Desktop Client Trigger.");
                SharedViewModel.IsRecognitionRunning = true;
                bool isFinished = await VXMusicActions.PerformRecognitionFlow();
                SharedViewModel.IsRecognitionRunning = false;
                //VXMusicOverlayInterface.StartVXMusicServerStream();
            }
        }

        public async Task<bool> PerformSaveAndTestShazamByoApi()
        {
            Logger.LogDebug("Saving and Testing Shazam BYOAPI Key");
            if (String.IsNullOrEmpty(_shazamByoApiToken))
                return false;

            VXUserSettings.Recognition.SetShazamByoApiKey(_shazamByoApiToken);
            VXUserSettings.Recognition.SetIsShazamByoApiEnabled(true);

            SharedViewModel.IsShazamApiConnected =
                await VXMusicSession.RecognitionClient.SetByoApiKeyAndTest(_shazamByoApiToken);

            return SharedViewModel.IsShazamApiConnected;
        }

        public async Task<bool> PerformSaveAndTestAudDByoApi()
        {
            if (String.IsNullOrEmpty(_audDByoApiToken))
                return false;

            Logger.LogDebug("Saving and Testing AudD BYOAPI Key");

            VXUserSettings.Recognition.SetAudDByoApiKey(_audDByoApiToken);
            VXUserSettings.Recognition.SetIsAudDByoApiEnabled(true);

            SharedViewModel.IsAudDApiConnected =
                await VXMusicSession.RecognitionClient.SetByoApiKeyAndTest(_audDByoApiToken);

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
            //IsAudDByoApiEnabled = false;

            ProcessRecognitionApiState();
            CheckIfCurrentApiIsConnected();
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.AudD);
            VXUserSettings.Recognition.SetRecognitionApi(RecognitionApi.AudD);

            SharedViewModel.IsShazamApiConnected = false;
            IsShazamApiEnabled = false;
            //IsShazamByoApiEnabled = false;

            ProcessRecognitionApiState();
            CheckIfCurrentApiIsConnected();
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

        private void OpenFolder(object commandParameter)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogsOutputPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Track Library: {ex.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}