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
    internal class SpotifyViewModel
    {
        private RelayCommand linkSpotifyButtonClick;

        public ICommand LinkSpotifyButtonClick => linkSpotifyButtonClick ??= new RelayCommand(PerformLinkSpotifyButtonClick);

        private void PerformLinkSpotifyButtonClick(object commandParameter)
        {
            VXMusicAPI.LinkSpotify();

        }
    }
}
