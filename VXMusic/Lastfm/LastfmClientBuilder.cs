using IF.Lastfm.Core.Api;

namespace VXMusic.Lastfm
{
    internal class LastfmClientBuilder
    {
        //private static readonly Lazy<Task<LastfmClient>> _lastfmClient = 
        //    new Lazy<Task<LastfmClient>>(() => CreateLastfmClient());

        public static HttpClient _httpClient;
        public static LastfmClient Instance;

        public async static Task<LastfmClient> CreateLastfmClient(string clientId, string clientSecret)
        {
            Instance = new LastfmClient(clientId, clientSecret);
            return Instance;
        }
        
        public static async Task<bool> Login(string username, string password)
        {
            var lastfm = LastfmClientBuilder.Instance;
            
            var lastResponse = await lastfm.Auth.GetSessionTokenAsync(username, password);
            return lastfm.Auth.Authenticated;
        }
    }
}
