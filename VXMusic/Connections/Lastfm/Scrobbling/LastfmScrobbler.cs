using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using IF.Lastfm.SQLite;
using Microsoft.Extensions.Logging;

namespace VXMusic.Lastfm.Scrobbling;

public class LastfmScrobbler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LastfmScrobbler> _logger;
    
    public static bool DbFileFileExists => File.Exists(_databasePath);

    private static readonly string _databasePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions",
            "VXMusic", "Cache", "vxscrobbles.db");

    public LastfmScrobbler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<LastfmScrobbler>)) 
            as ILogger<LastfmScrobbler> ?? throw new ApplicationException("A logger must be created in service provider.");

        if (!DbFileFileExists)
            File.Create(_databasePath);
    }
    
    public async Task<ScrobbleResponse> Scrobble(string artist, string album, string trackName)
    {
        _logger.LogTrace($"Scrobbling Track: {trackName} - {artist} - {album}");
        
        try
        {
            var lastfm = await LastfmClientBuilder.Instance;

            if (!lastfm.Auth.Authenticated)
            {
                _logger.LogWarning($"Last.fm has not been authenticated. Track will not scrobble.");
                return new ScrobbleResponse(LastResponseStatus.BadAuth);
            }

            IScrobbler scrobbler = new SQLiteScrobbler(lastfm.Auth, _databasePath);

            return await scrobbler.ScrobbleAsync(new Scrobble(artist, album, trackName, DateTimeOffset.Now));
        }
        catch (NullReferenceException)
        {
            _logger.LogError($"Failed to scrobble track. Check SQLite DB.");
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