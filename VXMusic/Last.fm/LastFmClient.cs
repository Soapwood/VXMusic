using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VXMusic.Last.fm
{
    internal class LastFmClient
    {
        public LastFmClient() 
        { 
        
        }

        public async static Task<LastAlbum> InitialiseLastFmClient()
        {
            //HttpClient httpClient = new HttpClient();
            var client = new LastfmClient("", "");
            //client.

            var response = await client.Album.GetInfoAsync("Grimes", "Visions");

            return response.Content;
        }
    }
}
