using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusic;
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
            var response = VXMusicAPI.LinkSpotify();
            if(response != null)
            {
                App.VXMusicSession.ConnectionsSettings.IsSpotifyConnected = true;
            }
        }

        private void PerformLinkLastfmButtonClick(object commandParameter)
        {
            var response = VXMusicAPI.LinkLastfm();
            if (response != null)
            {
                App.VXMusicSession.ConnectionsSettings.IsLastfmConnected = true;
            }
        }
    }
}
