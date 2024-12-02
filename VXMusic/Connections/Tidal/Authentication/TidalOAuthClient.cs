using System.Text;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using VXMusic.Connections.Tidal;

namespace VXMusic.Tidal.Authentication;

public class TidalOAuthClient : APIClient, IOAuthClient
{
    public Task<PKCETokenResponse> RequestToken(PKCETokenRequest request, CancellationToken cancel = default (CancellationToken)) => RequestToken(request, this.API, cancel);
    
    public static Task<PKCETokenResponse> RequestToken(
        PKCETokenRequest request,
        IAPIConnector apiConnector,
        CancellationToken cancel = default (CancellationToken))
    {
        TidalUtil.Ensure.ArgumentNotNull((object) request, nameof (request));
        TidalUtil.Ensure.ArgumentNotNull((object) apiConnector, nameof (apiConnector));
        List<KeyValuePair<string, string>> form = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("client_id", request.ClientId),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", request.Code),
            new KeyValuePair<string, string>("redirect_uri", request.RedirectUri.ToString()),
            new KeyValuePair<string, string>("code_verifier", request.CodeVerifier)
        };
        return SendOAuthRequest<PKCETokenResponse>(apiConnector, form, (string) null, (string) null, cancel);
    }
    
    private static Task<T> SendOAuthRequest<T>(
        IAPIConnector apiConnector,
        List<KeyValuePair<string?, string?>> form,
        string? clientId,
        string? clientSecret,
        CancellationToken cancel = default (CancellationToken))
    {
        // TODO Inject creds here
        Dictionary<string, string> headers = BuildAuthHeader(clientId, clientSecret);
        return apiConnector.Post<T>(TidalAuthentication.TidalAuthApiUrl, (IDictionary<string, string>) null, (object) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) form), headers, cancel);
    }
    
    private static Dictionary<string, string> BuildAuthHeader(string? clientId, string? clientSecret)
    {
        if (clientId == null || clientSecret == null)
            return new Dictionary<string, string>();
        return new Dictionary<string, string>()
        {
            {
                "Authorization",
                "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(clientId + ":" + clientSecret))
            }
        };
    }

    public TidalOAuthClient(SpotifyClientConfig config)
        : base((IAPIConnector) ValidateConfig(config))
    {
    }
    
    private static APIConnector ValidateConfig(SpotifyClientConfig config)
    {
        TidalUtil.Ensure.ArgumentNotNull((object) config, nameof (config));
        return new APIConnector(config.BaseAddress, config.Authenticator, config.JSONSerializer, config.HTTPClient, config.RetryHandler, config.HTTPLogger);
    }

    public Task<ClientCredentialsTokenResponse> RequestToken(ClientCredentialsRequest request, CancellationToken cancel = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<AuthorizationCodeRefreshResponse> RequestToken(AuthorizationCodeRefreshRequest request, CancellationToken cancel = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<AuthorizationCodeTokenResponse> RequestToken(AuthorizationCodeTokenRequest request, CancellationToken cancel = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<AuthorizationCodeTokenResponse> RequestToken(TokenSwapTokenRequest request, CancellationToken cancel = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<AuthorizationCodeRefreshResponse> RequestToken(TokenSwapRefreshRequest request, CancellationToken cancel = new CancellationToken())
    {
        throw new NotImplementedException();
    }
}