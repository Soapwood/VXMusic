using SpotifyAPI.Web;
using VXMusic.Lastfm.Authentication;
using VXMusic.Spotify;

namespace VXMusic.API
{
    /*
     * This shouldn't stay. Need to remove these calls as they're just quick patches for performing
     * logic that should be performed in the MVVM code anyway.
     */

    public class VXMusicAPI
    {
        public async static Task<PrivateUser> LinkSpotify()
        {
            var spotify = await SpotifyClientBuilder.CreateSpotifyClient();
            return await spotify.UserProfile.Current();
        }

        public async static Task<bool> LinkLastfm(string clientId, string clientSecret,
                                                        string username, string password)
        {
            LastfmAuthentication.ClientId = clientId;
            LastfmAuthentication.ClientSecret = clientSecret;
            
            //var last = await LastfmClientBuilder.CreateLastfmClient();
            return await LastfmAuthentication.Login(username, password);
        }
    }
}
