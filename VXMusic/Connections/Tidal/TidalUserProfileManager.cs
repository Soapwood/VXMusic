using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VXMusic.Spotify;
using VXMusic.Tidal;

namespace VXMusic.Connections.Tidal
{
    internal class TidalUserProfileManager
    {
        public static async Task<PrivateUser> GetCurrentUser()
        {
            // if (TidalClientBuilder.Instance == null)
            //     throw new Exception("Not authenticated with Tidal API.");  // TODO Handle this, and output to UI
            //
            // var tidal = await TidalClientBuilder.Instance;
            //
            // return await tidal.UserProfile.Current();
            return null;
        }
    }
}
