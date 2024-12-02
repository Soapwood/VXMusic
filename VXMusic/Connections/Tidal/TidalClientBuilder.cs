using VXMusic.Spotify.Authentication;
using VXMusic.Tidal.Authentication;
using System.ComponentModel;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using VXMusic.Tidal.Model;

namespace VXMusic.Tidal;

public class TidalClientBuilder
{
    private static readonly Lazy<Task<TidalClient>> _tidalClient =
        new Lazy<Task<TidalClient>>(() => CreateTidalClient());

    public static Task<TidalClient> Instance => _tidalClient.Value;

    public static async Task<TidalClient> CreateTidalClient()
    {
        await TidalAuthentication.GetTidalUserAuthentication();
        return new TidalClient();
    }
    
    public static SpotifyClientConfig CreateDefaultTidalConfig()
    {
        return new SpotifyClientConfig(TidalAuthentication.TidalAuthApiUrl, (IAuthenticator)null,
            (IJSONSerializer)new NewtonsoftJSONSerializer(), (IHTTPClient)new NetHttpClient(), (IRetryHandler)null,
            (IHTTPLogger)null, (IPaginator)new SimplePaginator());
    }
}

