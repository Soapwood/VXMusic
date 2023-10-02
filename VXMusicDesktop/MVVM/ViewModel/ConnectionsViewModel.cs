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
    internal class ConnectionsViewModel : INotifyPropertyChanged
    {
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
        public ICommand LinkLastfmButtonClick => linkLastfmButtonClick ??= new RelayCommand(PerformLinkLastfmButtonClick);

        private bool _isLastFmConnected = false;

        public ConnectionsViewModel() 
        {
            ShouldSpotifyLinkButtonBeEnabled = !SpotifyAuthentication.IsSpotifyConnected();
            SpotifyLinkButtonText = DetermineSpotifyLinkButtonStateContent();

            ShouldLastFmLinkButtonBeEnabled = !VXMusicSession.ConnectionsSettings.IsLastfmConnected;
            LastFmLinkButtonText = DetermineLastFmLinkButtonStateContent();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ShouldSpotifyLinkButtonBeEnabled
        {
            get { return !SpotifyAuthentication.IsSpotifyConnected(); }
            set
            {
                if (_shouldSpotifyLinkButtonBeShown != value)
                {
                    _shouldSpotifyLinkButtonBeShown = value;
                    OnPropertyChanged(nameof(ShouldSpotifyLinkButtonBeEnabled));
                }
            }
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

        private string DetermineSpotifyLinkButtonStateContent()
        {
            return ShouldSpotifyLinkButtonBeEnabled ? "Link Spotify" : "Connected!";
        }

        public string SpotifyLinkButtonText
        {
            get { return _spotifyLinkButtonText; }
            set
            {
                if (_spotifyLinkButtonText != value)
                {
                    _spotifyLinkButtonText = value;
                    OnPropertyChanged(nameof(ShouldSpotifyLinkButtonBeEnabled));
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
            if (!ShouldSpotifyLinkButtonBeEnabled)
                return;

            var response = VXMusicAPI.LinkSpotify(VXMusicSession.ConnectionsSettings.SpotifySettings.ClientId);
            if(response != null)
            {
                VXMusicSession.ConnectionsSettings.IsSpotifyConnected = true;
                ShouldSpotifyLinkButtonBeEnabled = false;
                SpotifyLinkButtonText = DetermineSpotifyLinkButtonStateContent();
                VXMusicSession.RaiseSpotifyLoggedIn();
            } else
            {

            }
        }

        private async void PerformLinkLastfmButtonClick(object commandParameter)
        {
            if (!ShouldLastFmLinkButtonBeEnabled)
                return;

            var response = await VXMusicAPI.LinkLastfm(VXMusicSession.ConnectionsSettings.LastfmSettings.ClientId,
                                                    VXMusicSession.ConnectionsSettings.LastfmSettings.ClientSecret,
                                                    LastFmUsername,
                                                    LastFmPassword);
            if (response)
            {
                _isLastFmConnected = true;
                VXMusicSession.ConnectionsSettings.IsLastfmConnected = true;
                VXMusicSession.RaiseLastFmLoggedIn();
            }
        }

        private string DetermineLastFmLinkButtonStateContent()
        {
            return ShouldLastFmLinkButtonBeEnabled ? "Login" : "Connected!";
        }

    }
}
