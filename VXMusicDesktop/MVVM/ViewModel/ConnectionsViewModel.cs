using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusic.API;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{
    internal class ConnectionsViewModel
    {
        private RelayCommand linkSpotifyButtonClick;
        private RelayCommand linkLastfmButtonClick;

        public ICommand LinkSpotifyButtonClick => linkSpotifyButtonClick ??= new RelayCommand(PerformLinkSpotifyButtonClick);
        public ICommand LinkLastfmButtonClick => linkLastfmButtonClick ??= new RelayCommand(PerformLinkLastfmButtonClick);

        private void PerformLinkSpotifyButtonClick(object commandParameter)
        {
            var response = VXMusicAPI.LinkSpotify(VXMusicSession.ConnectionsSettings.SpotifySettings.ClientId);
            if(response != null)
            {
                VXMusicSession.ConnectionsSettings.IsSpotifyConnected = true;
            }
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
