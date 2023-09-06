using System.Diagnostics;
using IF.Lastfm.Core.Scrobblers;
using SpotifyAPI.Web;
using VXMusic.Audio.Recording;
using VXMusic.Lastfm;
using VXMusic.Lastfm.Authentication;
using VXMusic.Lastfm.Scrobbling;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;
using VXMusic.Spotify;
using VXMusic.Spotify.Authentication;

namespace VXMusic.API
{
    
    public class VXMusicAPI
    {
        //VXMusicSession

        // public static void RunRecording()
        // {
        //     WindowsAudioDeviceListener recorder = new WindowsAudioDeviceListener();
        //
        //     recorder.StartRecording();
        //
        //     _xsOverlay.SendNotification("VXMusic is Listening...", "", recorder.RecordingTimeSeconds);
        //     Trace.WriteLine("Recording started.");
        //
        //     // Wait for the capture to complete by monitoring the capture state
        //     while (recorder.CurrentCaptureState != NAudio.CoreAudioApi.CaptureState.Stopped)
        //     {
        //         Thread.Sleep(500);
        //     }
        //
        //     recorder.StopRecording();
        //
        //     Trace.WriteLine("Recording stopped. Audio saved.");
        //     _xsOverlay.SendNotification("Sounds great! Just a moment..", "", 2);
        // }

        //public static IRecognitionClient SetRecognitionApi(RecognitionApi recognitionApi)
        //{
        //    switch (recognitionApi)
        //    {
        //        case RecognitionApi.Shazam:
        //            return new ShazamClient(); // 3-5 seconds
        //        case RecognitionApi.AudD:
        //            return new AudDClient(); // 10 seconds
        //        default:
        //            throw new ArgumentException("Invalid Recognition API Specified.");
        //    }
        //}

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

        //public async static Task<IApiClientResponse> RunRecognition()
        //{
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

        //   return null;
        //}

        public async static Task<PrivateUser> LinkSpotify(string clientId)
        {
            SpotifyAuthentication.ClientId = clientId;

            var spotify = await SpotifyClientBuilder.CreateSpotifyClient();
            return await spotify.UserProfile.Current();
        }

        public async static Task<bool> LinkLastfm(string clientId, string clientSecret,
                                                        string username, string password)
        {
            LastfmAuthentication.ClientId = clientId;
            LastfmAuthentication.ClientSecret = clientSecret;

            // TODO Only do this if it hasn't already been set up

            var last = await LastfmClientBuilder.CreateLastfmClient();
            return await LastfmClientBuilder.Login(username, password);
        }

        // public async static Task<ScrobbleResponse> Scrobble(string artist, string album, string trackName)
        // {
        //     var scrobbler = new LastfmScrobbler();
        //
        //     var response = await scrobbler.Scrobble(artist, album, trackName);
        //
        //     if (response.Success)
        //     {
        //         Trace.WriteLine("Successfully Scrobbled!");
        //     }
        //
        //     return response;
        // }

        public async static void ReportTrackToSpotifyPlaylist(IRecognitionApiClientResponse result)
        {
            var spotify = await SpotifyClientBuilder.Instance;

            var me = await spotify.UserProfile.Current();
            Trace.WriteLine($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");

            // get track name from output

            var searchRequest = new SearchRequest(SearchRequest.Types.Track, $"{result.Result.Artist} {result.Result.Title}");
            var searchResult = spotify.Search.Item(searchRequest);
            var uri = searchResult.Result.Tracks.Items[0].Uri;

            PlaylistAddItemsRequest playlistAddItemsRequest = new PlaylistAddItemsRequest(new List<string>()
            {
                //result.result.song_link
                uri
            });

            var playlists = await spotify.PaginateAll(await spotify.Playlists.CurrentUsers().ConfigureAwait(false));

            Trace.WriteLine($"Total Playlists in your Account: {playlists.Count}");

            var currentDate = DateTime.Now.ToString("dd/MM");
            var playlistName = $"{currentDate} - {LogParser.VRChat.VRChatLogParser.LastKnownLocationName}";

            var existingPlaylist = SpotifyPlaylistManager.GetPlaylistIdByNameIfExists(playlistName, playlists);

            if (existingPlaylist == null)
            {
                var response = await SpotifyPlaylistManager.CreatePlaylist(me.Id, playlistName, false); // TODO config isPublic above
                // TODO check if null
                await SpotifyPlaylistManager.AddTrackToPlaylist(response.Id, playlistAddItemsRequest);
            }
            else
            {
                await SpotifyPlaylistManager.AddTrackToPlaylist(existingPlaylist, playlistAddItemsRequest);
            }
        }
    }
}
