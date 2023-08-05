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
            Trace.WriteLine("Poggers");
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            Trace.WriteLine("Poggers");
        }

        private async void PerformListenButtonClick(object commandParameter)
        {
            VXMusicAPI.RunRecording();
            //var result = //await VXMusicAPI.RunRecognition();
            var result = await App.Session.RecognitionClient.RunRecognition();

            if (result.status == "error")
            {
                App.Session.NotificationClient.SendNotification("Recognition failed! Oh jaysus", "", 5);
                Trace.WriteLine("Recognition failed! Oh jaysus");
                //Environment.Exit(0);
            }
            else if (result.result == null)
            {
                App.Session.NotificationClient.SendNotification("Oops, couldn't get that.", "Tech Tip: Have you tried turning up your World Volume?", 5);
                Trace.WriteLine("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
                //Environment.Exit(0);
            }
            else
            {
                App.Session.NotificationClient.SendNotification(result.result.artist, result.result.title, 8);
                Trace.WriteLine($"{result.result.artist}: {result.result.title}");

                //return result;
            }

            VXMusicAPI.ReportTrackToSpotifyPlaylist(result);
        }
    }


}
