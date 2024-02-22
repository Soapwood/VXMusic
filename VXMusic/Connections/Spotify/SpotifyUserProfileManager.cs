using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXMusic.Spotify;

namespace VXMusic.Connections.Spotify
{
    internal class SpotifyUserProfileManager
    {
        public static async Task<PrivateUser> GetCurrentUser()
        {
            if (SpotifyClientBuilder.Instance == null)
                throw new Exception("Not authenticated with Spotify API.");  // TODO Handle this, and output to UI

            var spotify = await SpotifyClientBuilder.Instance;

            return await spotify.UserProfile.Current();
        }
    }
}
