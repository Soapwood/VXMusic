using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Diagnostics;
using System.Security.Authentication;

namespace VXMusic.Spotify.Authentication;

public class SpotifyAuthentication
{
    private const string CredentialsPath = "credentials.json";
    public static string? ClientId { get; set; } 

    private static readonly EmbedIOAuthServer _server = new(new Uri("http://localhost:5543/callback"), 5543);

    public static SpotifyClientConfig SpotifyClientConfig;

    private static void Exiting()
    {
        Console.CursorVisible = true;
    }

    public static async Task<int> GetSpotifyUserAuthentication()
    {
        // This is a bug in the SWAN Logging library, need this hack to bring back the cursor
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Exiting();

        if (string.IsNullOrEmpty(ClientId))
        {
            throw new AuthenticationException("Spotify ClientId has not been set." +
                                              "You can set it statically via SpotifyClientBuilder.ClientId");
        }

        if (File.Exists(CredentialsPath))
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

    private static async Task Start()
    {
        var json = await File.ReadAllTextAsync(CredentialsPath);
        var token = JsonConvert.DeserializeObject<PKCETokenResponse>(json);

        var authenticator = new PKCEAuthenticator(SpotifyAuthentication.ClientId!, token!);
        authenticator.TokenRefreshed += (sender, token) =>
            File.WriteAllText(CredentialsPath, JsonConvert.SerializeObject(token));

        var config = SpotifyClientConfig.CreateDefault()
            .WithAuthenticator(authenticator);

        SpotifyClientConfig = config;

        //var spotify = new SpotifyClient(config);

        // var me = await spotify.UserProfile.Current();
        // Console.WriteLine($"Welcome {me.DisplayName} ({me.Id}), you're authenticated!");
        //
        // var playlists = await spotify.PaginateAll(await spotify.Playlists.CurrentUsers().ConfigureAwait(false));
        // Console.WriteLine($"Total Playlists in your Account: {playlists.Count}");

        //_server.Dispose();
        //.Exit(0);
    }

    private static async Task StartAuthentication()
    {
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await _server.Start();
        _server.AuthorizationCodeReceived += async (sender, response) =>
        {
            await _server.Stop();
            var token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(SpotifyAuthentication.ClientId!, response.Code, _server.BaseUri, verifier)
            );

            await File.WriteAllTextAsync(CredentialsPath, JsonConvert.SerializeObject(token));
            await Start();
        };

        var request = new LoginRequest(_server.BaseUri, SpotifyAuthentication.ClientId!, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string>
            {
                Scopes.UserReadEmail,
                Scopes.UserReadPrivate,
                Scopes.PlaylistReadPrivate,
                Scopes.PlaylistReadCollaborative,
                Scopes.PlaylistModifyPrivate,
                Scopes.PlaylistModifyPublic,
            }
        };

        var uri = request.ToUri();
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