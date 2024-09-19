using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IF.Lastfm.Core.Scrobblers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using VXMusic;
using VXMusic.Spotify;
using VXMusic.Spotify.Authentication;
using VXMusicDesktop.Images;
using VXMusicDesktop.Theme;

namespace VXMusicDesktop;

public class VXMusicActions
{
    public static ILogger Logger = App.ServiceProvider.GetRequiredService<ILogger<VXMusicActions>>();

    public static async Task<bool> PerformRecognitionFlow()
    {
        await Task.Run(() =>
        {
            VXMusicSession.RecordingClient.StartRecording();

            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Info,"VXMusic is Listening...", "",
                VXMusicSession.RecordingClient.GetRecordingTimeSeconds());
            
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Info,"VXMusic is Listening...", "",
                VXMusicSession.RecordingClient.GetRecordingTimeSeconds());

            if (App.VXMusicSession.NotificationSettings.IsVRChatNotificationServiceEnabled)
            {
                App.VXMusicSession.VrChatNotification.SendNotificationAsync(NotificationLevel.Info, "VXMusic is Listening...", "", 3000);
                App.VXMusicSession.VrChatNotification.SendChatboxTypingIndicator(true);
            }
            
            while (!VXMusicSession.RecordingClient.IsCaptureStateStopped())
            {
                Thread.Sleep(500);
            }

            VXMusicSession.RecordingClient.StopRecording();
        });

        VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Info, "Sounds great! Just a moment..", "", 1);
        App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Info, "Sounds great! Just a moment..", "", 2);

        var result = await VXMusicSession.RecognitionClient.RunRecognition();

        if (result.Status == Status.Error)
        {
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Error,"Recognition failed! Oh jaysus", "", 5);
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Error,"Recognition failed! Oh jaysus", "", 5);
            Logger.LogError("Recognition failed! Oh jaysus");
        }
        else if (result.Status == Status.ApiError)
        {
            Logger.LogWarning("API Error. API Key is likely expired.");
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Warning,"API Error",
                "API Requests have Expired.", 5); 
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Warning,"API Error: ",
                "API Requests have Expired.", 5);
        }
        else if (result.Status == Status.NoMatches || result.Result == null)
        {
            Logger.LogWarning("Oops, couldn't get that. Tech Tip: Have you tried turning up your World Volume?");
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Warning,"Oops, couldn't get that.",
                "Tech Tip: Have you tried turning up your World Volume?", 5);            
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Warning,"Oops, couldn't get that.",
                "Tech Tip: Have you tried turning up your Media Volume?", 5);
            if(App.VXMusicSession.NotificationSettings.IsVRChatNotificationServiceEnabled)
                App.VXMusicSession.VrChatNotification.SendNotificationAsync(NotificationLevel.Warning, $"Couldn't catch that!", "Try turning up World Volume.", 3000);
        }
        else if (result.Status == Status.RecordingError)
        {
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Error,"I couldn't hear anything!",
                "Something messed up when recording audio. Check your audio device.", 10);
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Error,"I couldn't hear anything!",
                "Something messed up when recording audio. Check your audio device.", 10);
            Logger.LogError(
                "I couldn't hear anything! Something messed up when recording audio. Check your audio device.");
        }
        else
        {
            Logger.LogInformation(
                $"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Success,$"{result.Result.Artist} - {result.Result.Title}",
                $"{result.Result.Album} ({result.Result.ReleaseDate})", 8, result.Result.AlbumArt);
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Success,$"{result.Result.Artist} - {result.Result.Title}",
                $"{result.Result.Album} ({result.Result.ReleaseDate})", 8, result.Result.AlbumArt);
            
            if(App.VXMusicSession.NotificationSettings.IsVRChatNotificationServiceEnabled)
                App.VXMusicSession.VrChatNotification.SendNotificationAsync(NotificationLevel.Success,$"{result.Result.Artist} - {result.Result.Title}",
                    $"({result.Result.Album} - {result.Result.ReleaseDate})", 5000);
        }
        
        if(App.VXMusicSession.NotificationSettings.IsVRChatNotificationServiceEnabled)
            App.VXMusicSession.VrChatNotification.SendChatboxTypingIndicator(false);

        if (result.Result != null && SpotifyAuthentication.CurrentConnectionState == SpotifyConnectionState.Connected)
        {
            ReportTrackToSpotifyPlaylist(result);
            VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Success,"Spotify","Tracking Successful!", 1, IconImages.SpotifyLogo);
            App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Success,"Spotify","Tracking Successful!", 3);
        }
        

        if (result.Result != null)
        {
            VXMusicSession.PlaylistFileWriter.AddTrackEntryToPlaylistFile( GetPlaylistName(),
                $"{result.Result.Artist} - {result.Result.Title} - {result.Result.Album} ({result.Result.ReleaseDate})");

            if (App.VXMusicSession.ConnectionsSettings.IsLastfmConnected)
            {
                ScrobbleResponse lastfmResponse = await VXMusicSession.LastfmScrobbler.Scrobble(result.Result.Artist,
                    result.Result.Album, result.Result.Title);
                if (lastfmResponse.Success)
                {
                    Logger.LogInformation($"Successfully Scrobbled to Last.fm!");
                    VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Success, "Last.fm", "Successfully Scrobbled!", 1, IconImages.LastFmLogo);
                    App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Success, "Last.fm", "Successfully Scrobbled!", 3);
                }
                else
                {
                    Logger.LogWarning($"Scrobbling to Last.fm was not successful.");
                    VXMusicSession.NotificationClient.SendNotification(NotificationLevel.Success, "Last.fm", "Scrobbling Failed. Check your connection.", 3, IconImages.LastFmLogo);
                    App.VXMusicSession.ToastNotification.SendNotification(NotificationLevel.Error, "Last.fm", "Scrobbling Failed. Check your connection.", 3);
                }
            }
        }

        return true;
    }
    
    public async static void ReportTrackToSpotifyPlaylist(IRecognitionApiClientResponse result)
    {
        var spotify = await SpotifyClientBuilder.Instance;

        var me = await spotify.UserProfile.Current();
        Logger.LogInformation($"Spotify account {me.DisplayName} ({me.Id}) authenticated successfully.");

        // get track name from output

        var searchRequest = new SearchRequest(SearchRequest.Types.Track, $"{result.Result.Artist} {result.Result.Title}");
        var searchResult = spotify.Search.Item(searchRequest);
        var uri = searchResult.Result.Tracks.Items[0].Uri;

        PlaylistAddItemsRequest playlistAddItemsRequest = new PlaylistAddItemsRequest(new List<string>()
        {
            uri
        });

        IList<FullPlaylist> playlists = await spotify.PaginateAll(await spotify.Playlists.CurrentUsers().ConfigureAwait(false));

        Logger.LogTrace($"Total Playlists in Account: {playlists.Count}");
        
        var playlistName = GetPlaylistName();
        
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

    private static string GetPlaylistName()
    {
        Logger.LogTrace($"Getting playlist name..");
        
        var playlistName = "";
        
        var playlistSaveSetting = VXUserSettings.Connections.GetPlaylistSaveSetting();
        Logger.LogTrace($"Playlist Save Setting: {playlistSaveSetting}");

        if (playlistSaveSetting == PlaylistSaveSettings.Date)
        {
            // Prefix Playlist name with dd-MM date
            var currentDate = DateTime.Now.ToString("yy-MM-dd");
            playlistName += $"{currentDate}";
        }
        else if (playlistSaveSetting == PlaylistSaveSettings.Date_and_World)
        {
            // Prefix Playlist name with dd-MM date
            var currentDate = DateTime.Now.ToString("yy-MM-dd");
            playlistName += $"{currentDate}";
            
            var lastKnownLocationName = App.VXMusicSession.VRChatLogParser.CurrentVrChatWorld;

            if (lastKnownLocationName != null)
                playlistName += " [" + lastKnownLocationName + "]";
        }
        else if(playlistSaveSetting == PlaylistSaveSettings.Single_Playlist)
        {
            playlistName += "VXMusic";
        }

        return playlistName;
    }
    
}