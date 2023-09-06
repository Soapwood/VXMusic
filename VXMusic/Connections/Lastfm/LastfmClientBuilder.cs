using IF.Lastfm.Core.Api;
using VXMusic.Lastfm.Authentication;

namespace VXMusic.Lastfm
{
    internal class LastfmClientBuilder
    {
        private static readonly Lazy<Task<LastfmClient>> _lastfmClient = 
            new Lazy<Task<LastfmClient>>(() => CreateLastfmClient());

        public static HttpClient _httpClient;
        public static Task<LastfmClient> Instance = _lastfmClient.Value;
        public async static Task<LastfmClient> CreateLastfmClient()
        {
            return new LastfmClient(LastfmAuthentication.ClientId, LastfmAuthentication.ClientSecret);
        }
    }
}
