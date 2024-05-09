using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using VXMusic;
using VXMusic.Spotify;
using VXMusic.Spotify.Authentication;

namespace VXMusicDesktop;

public class VXMusicActions
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<App>>();

    public static async Task<bool> PerformRecognitionFlow()
    {
        // TODO Two recognitions can run at the same time, add check to disable button if it's already running

        await Task.Run(() =>
        {
            VXMusicSession.RecordingClient.StartRecording();

            VXMusicSession.NotificationClient.SendNotification("VXMusic is Listening...", "",
                VXMusicSession.RecordingClient.GetRecordingTimeSeconds());

            while (!VXMusicSession.RecordingClient.IsCaptureStateStopped())
            {
                Thread.Sleep(500);
            }

            VXMusicSession.RecordingClient.StopRecording();
        });

        VXMusicSession.NotificationClient.SendNotification("Sounds great! Just a moment..", "", 2);

        var result = await VXMusicSession.RecognitionClient.RunRecognition();

        if (result.Status == Status.Error)
        {
            VXMusicSession.NotificationClient.SendNotification("Recognition failed! Oh jaysus", "", 5);
            Logger.LogError("Recognition failed! Oh jaysus");
        }
        else if (result.Status == Status.NoMatches || result.Result == null)
        {
            VXMusicSession.NotificationClient.SendNotification("Oops, couldn't get that.",
                "Tech Tip: Have you tried turning up your World Volume?", 5);
            Logger.LogWarning("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
        }
        else if (result.Status == Status.RecordingError)
        {
            VXMusicSession.NotificationClient.SendNotification("I couldn't hear anything!",
                "Something messed up when recording audio. Check your audio device.", 10);
            Logger.LogError(
                "I couldn't hear anything! Something messed up when recording audio. Check your audio device.");
        }
        else
        {
            VXMusicSession.NotificationClient.SendNotification($"{result.Result.Artist} - {result.Result.Title}",
                $"{result.Result.Album} ({result.Result.ReleaseDate})", 8, result.Result.AlbumArt);
            Logger.LogInformation(
                $"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
        }

        if (result.Result != null && SpotifyAuthentication.CurrentConnectionState == SpotifyConnectionState.Connected)
            ReportTrackToSpotifyPlaylist(result);

        if (result.Result != null)
        {
            VXMusicSession.PlaylistFileWriter.AddLineToFileIfDateMatches(App.VXMusicSession.VRChatLogParser
                .CurrentVrChatWorld,
                $"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");

            if (App.VXMusicSession.ConnectionsSettings.IsLastfmConnected)
            {
                var lastfmResponse = await VXMusicSession.LastfmScrobbler.Scrobble(result.Result.Artist,
                    result.Result.Album, result.Result.Title);
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

        return true;
    }
    
    public async static void ReportTrackToSpotifyPlaylist(IRecognitionApiClientResponse result)
        {
            var spotify = await SpotifyClientBuilder.Instance;

            var me = await spotify.UserProfile.Current();
            Logger.LogInformation($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");

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

            Logger.LogTrace($"Total Playlists in Account: {playlists.Count}");

            // Prefix Playlist name with dd/MM date
            var currentDate = DateTime.Now.ToString("dd/MM");
            var playlistName = $"{currentDate}";
            
            var lastKnownLocationName = App.VXMusicSession.VRChatLogParser.CurrentVrChatWorld;

            if (lastKnownLocationName != null)
                playlistName += " - " + lastKnownLocationName;

            var existingPlaylist = SpotifyPlaylistManager.GetPlaylistIdByNameIfExists(playlistName, playlists);

            if (existingPlaylist == null)
            {
                var response = await SpotifyPlaylistManager.CreatePlaylist(me.Id, playlistName, false); // TODO config isPublic above
                
                Logger.LogInformation($"Adding Track {result.Result.Title} - {result.Result.Artist} to Playlist {playlistName}");
                await SpotifyPlaylistManager.AddTrackToPlaylist(response.Id, playlistAddItemsRequest);
            }
            else
            {
                Logger.LogInformation($"Adding Track {result.Result.Title} - {result.Result.Artist} to Playlist {playlistName}");
                await SpotifyPlaylistManager.AddTrackToPlaylist(existingPlaylist, playlistAddItemsRequest);
            }
        }
}