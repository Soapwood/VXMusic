using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using SpotifyAPI.Web;
using VXMusic.Lastfm.Authentication;
using VXMusic.Spotify;
using VXMusic.Spotify.Authentication;
using VXMusicDesktop.Core;

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
        
        public ICommand SetSpotifyPlaylistTrackingSetting => linkLastfmButtonClick ??= new RelayCommand(PerformSetSpotifyPlaylistSaveSetting);

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

        private async void PerformLastfmLogin(object commandParameter)
        {
            if (!ShouldLastFmLinkButtonBeEnabled)
                return;

            if (String.IsNullOrEmpty(LastFmUsername) || String.IsNullOrEmpty(LastFmPassword))
                return;

            VXUserSettings.Connections.SetLastfmUsername(LastFmUsername);
            VXUserSettings.Connections.SetLastfmPassword(LastFmPassword);

            LastfmAuthentication.ClientId = App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientId;
            LastfmAuthentication.ClientSecret = App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientSecret;
            
            var response = await LastfmAuthentication.Login(LastFmUsername, LastFmPassword);
            
            if (response)
            {
                SharedViewModel.IsLastFmConnected = true;
                App.VXMusicSession.ConnectionsSettings.IsLastfmConnected = true;
                VXMusicSession.RaiseLastFmLoggedIn();
            }
        }
        
        private async void PerformSetSpotifyPlaylistSaveSetting(object commandParameter)
        {

            if (String.IsNullOrEmpty(LastFmUsername) || String.IsNullOrEmpty(LastFmPassword))
                return;

            //VXUserSettings.Connections.SetSpotifyPlaylistSaveSetting(LastFmUsername);
            //App.VXMusicSession.ConnectionsSettings.LastfmSettings.ClientSecret;
            
        }

        private string DetermineLastFmLinkButtonStateContent()
        {
            return SharedViewModel.IsLastFmConnected ? "Connected!" : "Login";
        }

    }
}
