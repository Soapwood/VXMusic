using System.Security.Authentication;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using VXMusic.Spotify.Authentication;
using VXMusic.Spotify.Model;

namespace VXMusic.Spotify;

using System.Net;
using System.Text;
using Newtonsoft.Json;

public class SpotifyClientBuilder
{
    private static readonly Lazy<Task<SpotifyClient>> _spotifyClient =
        new Lazy<Task<SpotifyClient>>(() => CreateSpotifyClient());

    public static Task<SpotifyClient> Instance => _spotifyClient.Value;

    private readonly string _spotifyAccountEndpoint = "https://accounts.spotify.com/api/token";

    public static async Task<SpotifyClient> CreateSpotifyClient()
    {
        await SpotifyAuthentication.GetSpotifyUserAuthentication();
        return new SpotifyClient(SpotifyAuthentication.SpotifyClientConfig);
    }
}

