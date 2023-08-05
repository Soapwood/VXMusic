using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusic;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.MVVM.ViewModel
{

    internal class RecognitionViewModel
    {
        private RelayCommand shazamButtonClick;
        private RelayCommand audDButtonClick;
        private RelayCommand listenButtonClick;

        public ICommand ShazamButtonClick => shazamButtonClick ??= new RelayCommand(PerformShazamButtonClick);
        public ICommand AudDButtonClick => audDButtonClick ??= new RelayCommand(PerformAudDButtonClick);
        public ICommand ListenButtonClick => listenButtonClick ??= new RelayCommand(PerformListenButtonClick);

        // Concurrency fields


        private void PerformShazamButtonClick(object commandParameter)
        {
            Trace.WriteLine("Poggers");
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            Trace.WriteLine("Poggers");
        }

        private async void PerformListenButtonClick(object commandParameter)
        {
            VXMusicAPI.RunRecording();
            var result = await VXMusicAPI.RunRecognition();
            VXMusicAPI.ReportTrackToSpotifyPlaylist(result);
        }
    }


}
