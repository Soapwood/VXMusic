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
        
        private bool _isShazamApiEnabled;
        private bool _isRecognitionReady = true;
        
        #endregion
        
        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }

        #region Commands


        public ICommand ListenButtonClick => listenButtonClick ??= new RelayCommand(PerformListenButtonClick);
        public ICommand RecognitionViewLoaded => recognitionViewLoaded ??= new RelayCommand(OnRecognitionViewLoaded);

        public ICommand PasswordHintTextVisibilityToggle =>
            recognitionViewLoaded ??= new RelayCommand(OnRecognitionOptionChanged);

        public ICommand OpenFolderCommand => openFolderClick ??= new RelayCommand(OpenFolder);

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


        public bool IsShazamApiEnabled
        {
            get => _isShazamApiEnabled;
            set
            {
                _isShazamApiEnabled = value;
                OnPropertyChanged(nameof(IsShazamApiEnabled));
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

            HasBeenInitialised = true;

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
        }

        private void OnRecognitionOptionChanged(object commandParameter)
        {
            // No additional actions needed
        }
        
        private void OnSelectedDeviceChanged()
        {
            Logger.LogInformation($"Listening Audio Device changed to {SelectedAudioDevice}");
            
            WindowsAudioDeviceListener.SelectedAudioDevice = SelectedAudioDevice;
            VXUserSettings.Recognition.SetSelectedAudioDevice(SelectedAudioDevice.DeviceFriendlyName);
        }
        
        #endregion
        
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