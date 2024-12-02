using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Input;
using SpotifyAPI.Web;
using VXMusic.Lastfm.Authentication;
using VXMusic.Spotify;
using VXMusic.Spotify.Authentication;
using VXMusic.Tidal;
using VXMusic.Tidal.Authentication;
using VXMusicDesktop.Core;
using SpotifyConnectionState = VXMusic.Spotify.Authentication.SpotifyConnectionState;
using SpotifyConnectionStateExtensions = VXMusic.Spotify.Authentication.SpotifyConnectionStateExtensions;

namespace VXMusicDesktop.MVVM.ViewModel
{
    public class ConnectionsViewModel : INotifyPropertyChanged
    {
        // Shared ViewModel for sharing concurrency values between certain Views.
        public SharedViewModel SharedViewModel { get; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        private RelayCommand linkSpotifyButtonClick;
        private RelayCommand linkTidalButtonClick;
        private RelayCommand linkLastfmButtonClick;

        private bool _shouldSpotifyLinkButtonBeShown;
        private string _spotifyLinkButtonText;
        
        private string _tidalLinkButtonText;

        private string _lastFmUsername = "Username";
        private string _lastFmPassword = "Password";

        private bool _shouldLastFmLinkButtonBeShown;
        private string _lastFmLinkButtonText;

        public ICommand LinkSpotifyButtonClick => linkSpotifyButtonClick ??= new RelayCommand(PerformLinkSpotifyButtonClick);
        public ICommand LinkTidalButtonClick => linkTidalButtonClick ??= new RelayCommand(PerformLinkTidalButtonClick);
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
            LastfmAuthentication.ClientToken = App.VXMusicSession.ConnectionsSettings.LastfmSettings.AppToken;
            
            // TODO Check login here?
            var isLastFmConnected = await LastfmAuthentication.Login(App.VXMusicSession.ConnectionsSettings.LastfmSettings.Username,
                App.VXMusicSession.ConnectionsSettings.LastfmSettings.Password);

            SharedViewModel.IsLastFmConnected = isLastFmConnected;
            App.VXMusicSession.ConnectionsSettings.IsLastfmConnected = isLastFmConnected;
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

        public string TidalLinkButtonText 
        {
            get { return TidalConnectionStateExtensions.ToDisplayString(TidalAuthentication.CurrentConnectionState); }
            set
            {
                if (_tidalLinkButtonText != value)
                {
                    _tidalLinkButtonText = value;
                    OnPropertyChanged(nameof(TidalLinkButtonText));
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

            var response = LinkSpotify();

            if (response != null)
            {
                SpotifyAuthentication.RaiseSpotifyLoggingIn();
                SharedViewModel.IsSpotifyConnected = true;
            }
        }

        public async static Task<PrivateUser> LinkSpotify()
        {
            var spotify = await SpotifyClientBuilder.CreateSpotifyClient();
            return await spotify.UserProfile.Current();
        }
        
        private void PerformLinkTidalButtonClick(object commandParameter)
        {
            if (TidalAuthentication.CurrentConnectionState == TidalConnectionState.Connected)
                return;

            var response = LinkTidal();

            if (response != null)
            {
                TidalAuthentication.RaiseTidalLoggingIn();
                SharedViewModel.IsTidalConnected = true;
            }
        }
        
        public async static Task<PrivateUser> LinkTidal()
        {
            var tidal = await TidalClientBuilder.CreateTidalClient();
            //return await tidal.UserProfile.Current();
            return null;
        }

        private async void PerformLastfmLogin(object commandParameter)
        {
            if (!ShouldLastFmLinkButtonBeEnabled)
                return;

            if (String.IsNullOrEmpty(LastFmUsername) || String.IsNullOrEmpty(LastFmPassword))
                return;

            VXUserSettings.Connections.SetLastfmUsername(LastFmUsername);
            VXUserSettings.Connections.SetLastfmPassword(LastFmPassword);

            LastfmAuthentication.ClientId = App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientId;
            LastfmAuthentication.ClientToken = App.VXMusicSession.ConnectionsSettings.LastfmSettings.AppToken;
            
            var response = await LastfmAuthentication.Login(LastFmUsername, LastFmPassword);
            
            if (response)
            {
                SharedViewModel.IsLastFmConnected = true;
                App.VXMusicSession.ConnectionsSettings.IsLastfmConnected = true;
                VXMusicSession.RaiseLastFmLoggedIn();
            }
        }

        private string DetermineLastFmLinkButtonStateContent()
        {
            return SharedViewModel.IsLastFmConnected ? "Connected!" : "Login";
        }

    }
}
