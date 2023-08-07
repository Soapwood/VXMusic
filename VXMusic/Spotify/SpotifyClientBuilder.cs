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
    
    private string _spotifyAccountEndpoint = "https://accounts.spotify.com/api/token";
    
    private string _clientId = "52e2f3931eab490c99039b3217b697d7";
    
    private static async Task<SpotifyClient> CreateSpotifyClient()
    {
        //var spotifyClient = new SpotifyClientBuilder();
        //ClientCredentialsTokenResponse clientCredentials = await spotifyClient.GetAccessToken();
        await SpotifyAuthentication.GetSpotifyUserAuthentication();

        
        return new SpotifyClient(SpotifyAuthentication.SpotifyClientConfig);
    }

    // TODO Need to handle token refresh https://johnnycrazy.github.io/SpotifyAPI-NET/docs/client_credentials
    // private async Task<ClientCredentialsTokenResponse> GetAccessToken()
    // {
    //     var config = SpotifyClientConfig.CreateDefault();
    //     
    //     var request = new ClientCredentialsRequest(_clientId, _clientSecret);
    //     
    //     return await new OAuthClient(config).RequestToken(request);;
    // }
    
}

