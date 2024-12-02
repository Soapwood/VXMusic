using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Diagnostics;
using System.Security.Authentication;
using System.Web;
using VXMusic.Connections.Tidal;
using VXMusic.Spotify.Authentication;
using VXMusic.Tidal.Model;

namespace VXMusic.Tidal.Authentication;

public enum TidalConnectionState
{
    NotConnected,
    Connecting,
    Connected,
    ConnectionFailed
}

public static class TidalConnectionStateExtensions
{
    public static string ToDisplayString(this TidalConnectionState state)
    {
        switch (state)
        {
            case TidalConnectionState.NotConnected:
                return "Connect";
            case TidalConnectionState.Connecting:
                return "Check your browser";
            case TidalConnectionState.Connected:
                return "Connected!";
            case TidalConnectionState.ConnectionFailed:
                return "Connection Failed.";
            default:
                return "ERROR";
        }
    }
}


public class TidalAuthentication
{
    public static string CredentialsPath { get; set; }
    public static string CredentialsFilePath { get; set; }

    public static string? ClientId { get; set; } 

    private static readonly EmbedIOAuthServer _server = new(new Uri("http://localhost:5544/callback"), 5544);

    public static SpotifyClientConfig TidalClientConfig;

    public static event EventHandler TidalLogin;

    public static bool CredentialFileExists => File.Exists(CredentialsFilePath);

    public static TidalConnectionState CurrentConnectionState { get; set; }
    
    public static readonly Uri TidalApiUrl = new Uri("https://openapi.tidal.com");
    public static readonly Uri TidalAuthApiUrl = new Uri("https://auth.tidal.com/v1/oauth2/token");

    private static void Exiting()
    {
    }

    public static void RaiseTidalLoggingIn()
    {
        try
        {
            TidalLogin.Invoke(null, EventArgs.Empty);
        } catch (Exception e) {
            Trace.TraceError("TidalLogin delegate has not been created.");
        }
    }

    public static async Task<int> GetTidalUserAuthentication()
    {
        // This is a bug in the SWAN Logging library, need this hack to bring back the cursor
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Exiting();

        CurrentConnectionState = TidalConnectionState.Connecting;

        if (string.IsNullOrEmpty(ClientId))
        {
            throw new AuthenticationException("Tidal ClientId has not been set." +
                                              "You can set it statically via TidalClientBuilder.ClientId");
        }

        if (!Directory.Exists(CredentialsPath))
        {
            Directory.CreateDirectory(CredentialsPath);
        }

        if (File.Exists(CredentialsFilePath))
        {
            await Start();
        }
        else
        {
            await StartAuthentication();
        }


        await Task.Delay(1000);
        //_ = Console.ReadKey();

        return 0;
    }
    
    public static async Task<bool> CheckIfTidalIsConnected()
    {
        //await GetSpotifyUserAuthentication();

        try
        {
            var currentUser = await TidalUserProfileManager.GetCurrentUser();
            CurrentConnectionState = TidalConnectionState.Connected;

            return true;
        } 
        catch (SpotifyAPI.Web.APIUnauthorizedException ex)
        {
            throw new SpotifyAPI.Web.APIUnauthorizedException("Spotify is not connected - user not found.");
            //return false;
        }
    }

    private static async Task Start()
    {
        var json = await File.ReadAllTextAsync(CredentialsFilePath);
        var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);
        
        var authenticator = new PKCEAuthenticator(TidalAuthentication.ClientId!, token!);
        authenticator.TokenRefreshed += (sender, token) =>
            File.WriteAllText(CredentialsFilePath, JsonConvert.SerializeObject(token));

        //var config = SpotifyClientConfig.CreateDefault()
           // .WithAuthenticator(authenticator);

        //TidalClientConfig = config;

        CurrentConnectionState = TidalConnectionState.Connected;

        RaiseTidalLoggingIn();
    }

    private static async Task StartAuthentication()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await _server.Start();
        _server.AuthorizationCodeReceived += async (sender, response) =>
        {
            await _server.Stop();
            
            TidalClientConfig = TidalClientBuilder.CreateDefaultTidalConfig();
            
            var token = await new TidalOAuthClient(TidalClientConfig).RequestToken(
                new PKCETokenRequest(TidalAuthentication.ClientId!, response.Code, _server.BaseUri, verifier)
            );

            await File.WriteAllTextAsync(CredentialsFilePath, JsonConvert.SerializeObject(token));
            await Start();
        };

        var request = new LoginRequest(_server.BaseUri, TidalAuthentication.ClientId!, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string>
            {
                TidalScopes.PlaylistRead,
                //TidalScopes.PlaylistWrite
            }
        };

        // TODO Use a proper stringbuilder
        Uri uri = new Uri($"https://login.tidal.com/authorize?response_type=code" +
                          $"&client_id={TidalAuthentication.ClientId}" +
                          $"&redirect_uri={_server.BaseUri}" +
                          $"&scope={HttpUtility.UrlEncode(string.Join(" ", (IEnumerable<string>) request.Scope))}" +
                          $"&code_challenge={request.CodeChallenge}" +
                          $"&code_challenge_method={request.CodeChallengeMethod}");

        try
        {
            BrowserUtil.Open(uri);
        }
        catch (Exception)
        {
            Trace.WriteLine($"Unable to open URL, manually open: {uri}");
        }
    }
}