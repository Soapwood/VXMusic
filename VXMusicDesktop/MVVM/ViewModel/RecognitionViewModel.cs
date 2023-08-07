using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using VXMusic;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;
using VXMusicDesktop.Core;
using VXMusicDesktop;

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
        public static String CurrentRecognitionApi = RecognitionSettings.GetRecognitionApiString();
        public static bool ShazamApi = true;


        private void PerformShazamButtonClick(object commandParameter)
        {
            App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi = RecognitionApi.Shazam;
            App.VXMusicSession.RecognitionClient = new ShazamClient();
            Trace.WriteLine("Poggers");
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi = RecognitionApi.AudD;
            App.VXMusicSession.RecognitionClient = new AudDClient();
            Trace.WriteLine("Poggers");
        }

        private async void PerformListenButtonClick(object commandParameter)
        {
            // TODO Two recognitions can run at the same time, add check to disable button if it's already running
            VXMusicAPI.RunRecording();
            //var result = //await VXMusicAPI.RunRecognition();
            var result = await App.VXMusicSession.RecognitionClient.RunRecognition();

            if (result.Status == Status.Error)
            {
                App.VXMusicSession.NotificationClient.SendNotification("Recognition failed! Oh jaysus", "", 5);
                Trace.WriteLine("Recognition failed! Oh jaysus");
                //Environment.Exit(0);
            }
            else if (result.Status == Status.NoMatches || result.Result == null)
            {
                App.VXMusicSession.NotificationClient.SendNotification("Oops, couldn't get that.", "Tech Tip: Have you tried turning up your World Volume?", 5);
                Trace.WriteLine("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
            }
            else
            {
                App.VXMusicSession.NotificationClient.SendNotification($"{result.Result.Artist} - {result.Result.Title}", $"{result.Result.Album} ({result.Result.ReleaseDate})", 8);
                Trace.WriteLine($"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
            }

            if(App.VXMusicSession.SpotifySettings.IsSpotifyConnected)
                VXMusicAPI.ReportTrackToSpotifyPlaylist(result);
        }
    }


}
