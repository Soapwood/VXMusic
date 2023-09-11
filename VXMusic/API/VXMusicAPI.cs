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
    /*
     * This shouldn't stay. Need to remove these calls as they're just quick patches for performing
     * logic that should be performed in the MVVM code anyway.
     */

    public class VXMusicAPI
    {
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
            return await LastfmAuthentication.Login(username, password);
        }

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

            // Prefix Playlist name with dd/MM date
            var currentDate = DateTime.Now.ToString("dd/MM");
            var playlistName = $"{currentDate}";
            
            // TODO Only run this if VRChat is running
            var lastKnownLocationName = LogParser.VRChat.VRChatLogParser.LastKnownLocationName;

            if (lastKnownLocationName != null)
                playlistName += " - " + lastKnownLocationName;

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
