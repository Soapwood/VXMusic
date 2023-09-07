using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusic.API;
using VXMusic.Spotify.Authentication;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    internal class ConnectionsViewModel : INotifyPropertyChanged
    {
        private RelayCommand linkSpotifyButtonClick;
        private RelayCommand linkLastfmButtonClick;

        private bool _shouldSpotifyLinkButtonBeShown;
        private string _spotifyLinkButtonText;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand LinkSpotifyButtonClick => linkSpotifyButtonClick ??= new RelayCommand(PerformLinkSpotifyButtonClick);
        public ICommand LinkLastfmButtonClick => linkLastfmButtonClick ??= new RelayCommand(PerformLinkLastfmButtonClick);

        public ConnectionsViewModel() 
        {
            ShouldSpotifyLinkButtonBeEnabled = !SpotifyAuthentication.IsSpotifyConnected();
            SpotifyLinkButtonText = DetermineSpotifyLinkButtonStateContent();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool ShouldSpotifyLinkButtonBeEnabled
        {
            get { return !SpotifyAuthentication.IsSpotifyConnected(); }
            //get { return _isSpotifyConnected; }
            set
            {
                if (_shouldSpotifyLinkButtonBeShown != value)
                {
                    _shouldSpotifyLinkButtonBeShown = value;
                    OnPropertyChanged(nameof(ShouldSpotifyLinkButtonBeEnabled));
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

        private void PerformLinkSpotifyButtonClick(object commandParameter)
        {
            var response = VXMusicAPI.LinkSpotify(VXMusicSession.ConnectionsSettings.SpotifySettings.ClientId);
            if(response != null)
            {
                VXMusicSession.ConnectionsSettings.IsSpotifyConnected = true;
            }

            ShouldSpotifyLinkButtonBeEnabled = false;
            SpotifyLinkButtonText = DetermineSpotifyLinkButtonStateContent();
        }

        private void PerformLinkLastfmButtonClick(object commandParameter)
        {
            var response = VXMusicAPI.LinkLastfm(VXMusicSession.ConnectionsSettings.LastfmSettings.ClientId,
                                                    VXMusicSession.ConnectionsSettings.LastfmSettings.ClientSecret,
                                                    VXMusicSession.ConnectionsSettings.LastfmSettings.Username,
                                                    VXMusicSession.ConnectionsSettings.LastfmSettings.Password);
            if (response != null)
            {
                VXMusicSession.ConnectionsSettings.IsLastfmConnected = true;
            }
        }


    }
}
