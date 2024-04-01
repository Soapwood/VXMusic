using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusic;
using VXMusic.API;
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

        //var result = //await VXMusicAPI.RunRecognition();
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
                $"{result.Result.Album} ({result.Result.ReleaseDate})", 8);
            Logger.LogInformation(
                $"{result.Result.Artist} - {result.Result.Title} {result.Result.Album} ({result.Result.ReleaseDate})");
        }

        if (result.Result != null && SpotifyAuthentication.CurrentConnectionState == SpotifyConnectionState.Connected)
            VXMusicAPI.ReportTrackToSpotifyPlaylist(result);

        if (result.Result != null)
        {
            VXMusicSession.PlaylistFileWriter.AddLineToFileIfDateMatches(
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
}