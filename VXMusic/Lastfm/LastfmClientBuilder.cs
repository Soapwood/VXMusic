using IF.Lastfm.Core.Api;

namespace VXMusic.Lastfm
{
    internal class LastfmClientBuilder
    {
        private static readonly Lazy<Task<LastfmClient>> _lastfmClient = 
            new Lazy<Task<LastfmClient>>(() => CreateLastfmClient());

        public static HttpClient _httpClient;
        public static Task<LastfmClient> Instance => _lastfmClient.Value;

        private static readonly string _apiKey = "";
        private static readonly string _apiSecret = "";

        public async static Task<LastfmClient> CreateLastfmClient()
        {
            return new LastfmClient(_apiKey, _apiSecret);
        }
        
        public static async Task<bool> Login(string username, string password)
        {
            var lastfm = await LastfmClientBuilder.Instance;
            
            var lastResponse = await lastfm.Auth.GetSessionTokenAsync(username, password);
            return lastfm.Auth.Authenticated;
        }
    }
}
