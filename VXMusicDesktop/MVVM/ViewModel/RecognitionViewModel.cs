using System;
using System.Threading;
using System.Windows.Input;
using VXMusic;
using VXMusicDesktop.Core;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic.API;
using VXMusic.FileWriter;
using System.Threading.Tasks;

namespace VXMusicDesktop.MVVM.ViewModel
{

    internal class RecognitionViewModel : INotifyPropertyChanged
    {
        public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();

        private RelayCommand shazamButtonClick;
        private RelayCommand audDButtonClick;
        private RelayCommand listenButtonClick;

        private RelayCommand recognitionViewLoaded;

        public ICommand ShazamButtonClick => shazamButtonClick ??= new RelayCommand(PerformShazamButtonClick);
        public ICommand AudDButtonClick => audDButtonClick ??= new RelayCommand(PerformAudDButtonClick);
        public ICommand ListenButtonClick => listenButtonClick ??= new RelayCommand(PerformListenButtonClick);
        public ICommand RecognitionViewLoaded => recognitionViewLoaded ??= new RelayCommand(OnRecognitionViewLoaded);

        // Concurrency fields
        public static String CurrentRecognitionApi;
        public static bool ShazamApi = true;

        public static String AudDButtonText = "Donkey fungus";

        // // //
        public event PropertyChangedEventHandler? PropertyChanged;
        
        private bool _isShazamApiEnabled;
        private bool _isAudDApiEnabled;

        private bool _isRecognitionReady = true;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsShazamApiEnabled
        {
            get { return _isShazamApiEnabled; }
            set
            {
                _isShazamApiEnabled = value;
                OnPropertyChanged(nameof(IsShazamApiEnabled));
            }
        }

        public bool IsAudDApiEnabled
        {
            get { return _isAudDApiEnabled; }
            set
            {
                _isAudDApiEnabled = value;
                OnPropertyChanged(nameof(IsAudDApiEnabled)); 
            }
        }

        public bool IsRecognitionReady
        {
            get { return _isRecognitionReady; }
            set
            {
                _isRecognitionReady = value;
                OnPropertyChanged(nameof(IsRecognitionReady)); 
            }
        }

        private void OnRecognitionViewLoaded(object commandParameter)
        {
            ProcessRecognitionApiState();
        }

        private void ProcessRecognitionApiState()
        {
            switch (App.VXMusicSession.RecognitionSettings.CurrentRecognitionApi)
            {
                case RecognitionApi.Shazam:
                    IsShazamApiEnabled = true;
                    IsAudDApiEnabled = false;
                    break;
                case RecognitionApi.AudD:
                    IsAudDApiEnabled = true;
                    IsShazamApiEnabled = false;
                    break;
                default:
                    IsShazamApiEnabled = false;
                    IsAudDApiEnabled = false;
                    break;
            }

            _isRecognitionReady = IsShazamApiEnabled || IsAudDApiEnabled;
        }

        private void PerformShazamButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.Shazam);
            RecognitionSettings.SetRecognitionApiInSettings(RecognitionApi.Shazam);
            ProcessRecognitionApiState();
        }

        private void PerformAudDButtonClick(object commandParameter)
        {
            App.VXMusicSession.SetRecognitionClient(RecognitionApi.AudD);
            RecognitionSettings.SetRecognitionApiInSettings(RecognitionApi.AudD);
            ProcessRecognitionApiState();
        }

        private async void PerformListenButtonClick(object commandParameter)
        {
            // TODO Two recognitions can run at the same time, add check to disable button if it's already running

            await Task.Run(() =>
            {
                VXMusicSession.RecordingClient.StartRecording();

                VXMusicSession.NotificationClient.SendNotification("VXMusic is Listening...", "", VXMusicSession.RecordingClient.GetRecordingTimeSeconds());

                while (!VXMusicSession.RecordingClient.IsCaptureStateStopped())
                {
                    Thread.Sleep(500);
                }

                VXMusicSession.RecordingClient.StopRecording();
            });
            
            VXMusicSession.NotificationClient.SendNotification("Sounds great! Just a moment..", "", 2);
            
            //var result = //await VXMusicAPI.RunRecognition();
            var result = await VXMusicSession.RecognitionClient.RunRecognition();

            if (result.Status == Status.Error)
            {
                VXMusicSession.NotificationClient.SendNotification("Recognition failed! Oh jaysus", "", 5);
                Logger.LogError("Recognition failed! Oh jaysus");
                //Environment.Exit(0);
            }
            else if (result.Status == Status.NoMatches || result.Result == null)
            {
                VXMusicSession.NotificationClient.SendNotification("Oops, couldn't get that.", "Tech Tip: Have you tried turning up your World Volume?", 5);
                Logger.LogWarning("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
            }
            else if (result.Status == Status.RecordingError)
            {
                VXMusicSession.NotificationClient.SendNotification("I couldn't hear anything!", "Something messed up when recording audio. Check your audio device.", 10);
                Logger.LogError("I couldn't hear anything! Something messed up when recording audio. Check your audio device.");
            }
            else
            {
                VXMusicSession.NotificationClient.SendNotification($"{result.Result.Artist} - {result.Result.Title}", $"{result.Result.Album} ({result.Result.ReleaseDate})", 8);
                Logger.LogInformation($"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
            }

            if(result.Result != null && VXMusicSession.ConnectionsSettings.IsSpotifyConnected)
                VXMusicAPI.ReportTrackToSpotifyPlaylist(result);

            if (result.Result != null)
            {
                VXMusicSession.PlaylistFileWriter.AddLineToFileIfDateMatches($"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
                
                var lastfmResponse = await VXMusicSession.LastfmScrobbler.Scrobble(result.Result.Artist, result.Result.Album, result.Result.Title);
                if (lastfmResponse.Success)
                {
                    Logger.LogInformation($"Successfully Scrobbled to Last.fm!");
                    VXMusicSession.NotificationClient.SendNotification("Last.fm", "Successfully Scrobbled!", 2);
                }
                else
                {
                    Logger.LogWarning($"Scrobbling to Last.fm was not successful.");
                    VXMusicSession.NotificationClient.SendNotification("Last.fm", "Scrobbling was not successful.", 2);
                }
            }
        }
    }


}
