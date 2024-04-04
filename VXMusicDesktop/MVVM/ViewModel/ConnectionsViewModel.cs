using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using VXMusic.API;
using VXMusic.Lastfm.Authentication;
using VXMusic.Spotify.Authentication;
using VXMusicDesktop.Core;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class ConnectionsViewModel : INotifyPropertyChanged
    {
        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private RelayCommand linkSpotifyButtonClick;
        private RelayCommand linkLastfmButtonClick;

        private bool _shouldSpotifyLinkButtonBeShown;
        private string _spotifyLinkButtonText;

        private string _lastFmUsername = "Username";
        private string _lastFmPassword = "Password";

        private bool _shouldLastFmLinkButtonBeShown;
        private string _lastFmLinkButtonText;

        public ICommand LinkSpotifyButtonClick => linkSpotifyButtonClick ??= new RelayCommand(PerformLinkSpotifyButtonClick);
        public ICommand LinkLastfmButtonClick => linkLastfmButtonClick ??= new RelayCommand(PerformLastfmLogin);

        private bool _isLastFmConnected = false;

        public ConnectionsViewModel(SharedViewModel sharedViewModel)
        {
            SharedViewModel = sharedViewModel;
            Initialise();
        }

        public void Initialise()
        {
            if (SpotifyAuthentication.CredentialFileExists)
                IsSpotifyConnected();

            if (!String.IsNullOrEmpty(App.VXMusicSession.ConnectionsSettings.LastfmSettings.Username) && 
                !String.IsNullOrEmpty(App.VXMusicSession.ConnectionsSettings.LastfmSettings.Password))
                IsLastFmConnected();
            
            ShouldLastFmLinkButtonBeEnabled = App.VXMusicSession.ConnectionsSettings.IsLastfmConnected;
            LastFmLinkButtonText = DetermineLastFmLinkButtonStateContent();
        }

        public async Task IsLastFmConnected()
        {
            LastfmAuthentication.ClientId = App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientId;
            LastfmAuthentication.ClientSecret = App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientSecret;

            SharedViewModel.IsLastFmConnected = await LastfmAuthentication.Login(App.VXMusicSession.ConnectionsSettings.LastfmSettings.Username,
                App.VXMusicSession.ConnectionsSettings.LastfmSettings.Password);
        }

        public async Task IsSpotifyConnected()
        {
            SharedViewModel.IsSpotifyConnected = await SpotifyAuthentication.CheckIfSpotifyIsConnected();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ShouldLastFmLinkButtonBeEnabled
        {
            get { return !_isLastFmConnected; }
            set
            {
                if (_isLastFmConnected != value)
                {
                    _shouldSpotifyLinkButtonBeShown = value;
                    OnPropertyChanged(nameof(ShouldLastFmLinkButtonBeEnabled));
                }
            }
        }

        public string LastFmUsername
        {
            get { return _lastFmUsername; }
            set
            {
                if (_lastFmUsername != value)
                {
                    _lastFmUsername = value;
                    OnPropertyChanged(nameof(LastFmUsername));
                }
            }
        }

        public string LastFmPassword
        {
            get { return _lastFmPassword; }
            set
            {
                if (_lastFmPassword != value)
                {
                    _lastFmPassword = value;
                    OnPropertyChanged(nameof(LastFmPassword));
                }
            }
        }
        
        public string SpotifyLinkButtonText
        {
            get { return SpotifyConnectionStateExtensions.ToDisplayString(SpotifyAuthentication.CurrentConnectionState); }
            set
            {
                if (_spotifyLinkButtonText != value)
                {
                    _spotifyLinkButtonText = value;
                    OnPropertyChanged(nameof(SpotifyLinkButtonText));
                }
            }
        }

        public string LastFmLinkButtonText
        {
            get { return _lastFmLinkButtonText; }
            set
            {
                if (_lastFmLinkButtonText != value)
                {
                    _lastFmLinkButtonText = value;
                    OnPropertyChanged(nameof(LastFmLinkButtonText));
                }
            }
        }

        private void PerformLinkSpotifyButtonClick(object commandParameter)
        {
            if (SpotifyAuthentication.CurrentConnectionState == SpotifyConnectionState.Connected)
                return;

            var response = VXMusicAPI.LinkSpotify();
            if(response != null)
            {
                //VXMusicSession.ConnectionsSettings.IsSpotifyConnected = true;
                //ShouldSpotifyLinkButtonBeEnabled = false;
                //SpotifyLinkButtonText = DetermineSpotifyLinkButtonStateContent();
                SpotifyAuthentication.RaiseSpotifyLoggingIn();
                SharedViewModel.IsSpotifyConnected = true;
            } else
            {

            }
        }

        private async void PerformLastfmLogin(object commandParameter)
        {
            if (!ShouldLastFmLinkButtonBeEnabled)
                return;

            if (String.IsNullOrEmpty(LastFmUsername) || String.IsNullOrEmpty(LastFmPassword))
                return;

            VXUserSettings.Connections.SetLastfmUsername(LastFmUsername);
            VXUserSettings.Connections.SetLastfmPassword(LastFmPassword);

            var response = await VXMusicAPI.LinkLastfm(App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientId,
                                                    App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientSecret,
                                                    LastFmUsername,
                                                    LastFmPassword);
            if (response)
            {
                SharedViewModel.IsLastFmConnected = true;
                App.VXMusicSession.ConnectionsSettings.IsLastfmConnected = true;
                VXMusicSession.RaiseLastFmLoggedIn();
            }
        }

        private string DetermineLastFmLinkButtonStateContent()
        {
            return ShouldLastFmLinkButtonBeEnabled ? "Login" : "Connected!";
        }

    }
}
