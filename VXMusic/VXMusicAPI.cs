using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;
using VXMusic.Spotify;

namespace VXMusic
{
    
    public enum RecognitionApi {
        Shazam,
        AudD
    }

    public enum NotificationService
    {
        XSOverlay,
        SteamVR
    }


    
    public class VXMusicAPI
    {
        private static XSOverlay _xsOverlay = new XSOverlay();
        //VXMusicSession

        public static void RunRecording()
        { 
            WindowsAudioDeviceListener recorder = new WindowsAudioDeviceListener();

            recorder.StartRecording();
            
            _xsOverlay.SendNotification("VXMusic is Listening...", "", recorder.RecordingTimeSeconds);
            Trace.WriteLine("Recording started.");

            // Wait for the capture to complete by monitoring the capture state
            while (recorder.CurrentCaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
            {
                Thread.Sleep(500);
            }

            recorder.StopRecording();

            Trace.WriteLine("Recording stopped. Audio saved.");
            _xsOverlay.SendNotification("Sounds great! Just a moment..", "", 2);
        }

        public static IRecognitionClient SetRecognitionApi(RecognitionApi recognitionApi)
        {
            switch (recognitionApi)
            {
                case RecognitionApi.Shazam:
                    return new ShazamClient(); // 3-5 seconds
                case RecognitionApi.AudD:
                    return new AudDClient(); // 10 seconds
                default:
                    throw new ArgumentException("Invalid Recognition API Specified.");
            }
        }

        public static INotificationClient SetNotificationClient(NotificationService notificationService)
        {
            switch (notificationService)
            {
                case NotificationService.XSOverlay:
                    return new XSOverlay();
                case NotificationService.SteamVR:
                    throw new NotImplementedException("SteamVR Notification system not Implemented yet.");
                default:
                    throw new ArgumentException("Invalid Notification Service specified.");
            }
        }
        
        public async static Task<IApiClientResponse> RunRecognition()
        {
            //var shazamClient = new ShazamClient(); // 3-5 seconds
            //var result = await shazamClient.RunRecognition();

            //var audDClient = new AudDClient();
            //var result = await audDClient.RunRecognition();

            //if (result.status == "error")
            //{
            //    _xsOverlay.XSNotification("Recognition failed! Oh jaysus", "", 5);
            //    Trace.WriteLine("Recognition failed! Oh jaysus");
            //    Environment.Exit(0);
            //} else if (result.result == null)
            //{
            //    _xsOverlay.XSNotification("Oops, couldn't get that.", "Tech Tip: Have you tried turning up your World Volume?", 5);
            //    Trace.WriteLine("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
            //    Environment.Exit(0);
            //} else
            //{
            //    _xsOverlay.XSNotification(result.result.artist, result.result.title, 8);
            //    Trace.WriteLine($"{result.result.artist}: {result.result.title}");

            //    return result;
            //}

            return null;
        }

        public async static Task<PrivateUser> LinkSpotify()
        {
            var spotify = await SpotifyClientBuilder.Instance;
            return await spotify.UserProfile.Current();
        }        
        
        public async static void ReportTrackToSpotifyPlaylist(IApiClientResponse result)
        {
            var spotify = await SpotifyClientBuilder.Instance;

            var me = await spotify.UserProfile.Current();
            Trace.WriteLine($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");

            var playlists = await spotify.PaginateAll(await spotify.Playlists.CurrentUsers().ConfigureAwait(false));
            Trace.WriteLine($"Total Playlists in your Account: {playlists.Count}");

            var newPlaylist = new PlaylistCreateRequest("VXMusic");
//newPlaylist.Public = false;
            newPlaylist.Description = "This playlist was created with VXMusic.";

            FullPlaylist fullPlaylist = await spotify.Playlists.Create(me.Id, new PlaylistCreateRequest("VXMusic"));

// get track name from shazam output

            var searchRequest = new SearchRequest(SearchRequest.Types.Track, $"{result.result.artist} {result.result.title}");
            var searchResult = spotify.Search.Item(searchRequest);
            var uri = searchResult.Result.Tracks.Items[0].Uri;

            PlaylistAddItemsRequest playlistAddItemsRequest = new PlaylistAddItemsRequest(new List<string>()
            {
                //result.result.song_link
                uri
            });

            var itemAddResponse = await spotify.Playlists.AddItems(fullPlaylist.Id, playlistAddItemsRequest);
            //var uploadResponse = await spotify.Playlists.UploadCover(fullPlaylist.Id, "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");

        }
    }
}
