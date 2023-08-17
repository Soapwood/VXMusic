using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using IF.Lastfm.SQLite;

namespace VXMusic.Lastfm.Scrobbling;

public class LastfmScrobbler
{
    private static readonly string _databasePath = "C:\\Users\\Tam\\Desktop\\vxscrobbles.db";

    public async Task<ScrobbleResponse> Scrobble(string artist, string album, string trackName)
    {
        try
        {
            var lastfm = await LastfmClientBuilder.Instance;

            if (!lastfm.Auth.Authenticated)
            {
                return new ScrobbleResponse(LastResponseStatus.BadAuth);
            }

            // TODO Create DB on boot of application. May want to use it for other things.

            //var result = File.Create(_databasePath);
            //File.Create()
            //IScrobbler scrobbler = new VXMusicScrobbler(LastfmClient.Auth, databasePath);
            IScrobbler scrobbler = new SQLiteScrobbler(lastfm.Auth, _databasePath);

            return await scrobbler.ScrobbleAsync(new Scrobble(artist, album, trackName, DateTimeOffset.Now));
        }
        catch (NullReferenceException)
        {
            return new ScrobbleResponse(LastResponseStatus.Failure);
        }
    }

    //public async static Task<bool> GetRecentScrobbles()
    //{
    //    var client = await LastfmClientBuilder.Instance;

    //    var recentScrobbles = await client.User.GetRecentScrobbles(client.Auth.UserSession.Username,
    //        null, null, false, 1, 5);

    //    Trace.WriteLine("Found Tracks:");
    //    foreach (var scrobble in recentScrobbles)
    //    {
    //        Trace.WriteLine($"{scrobble.Name} - {scrobble.ArtistName}");
    //    }

    //    return true;
    //}
}