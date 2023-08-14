using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using SharpDX;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using System.Diagnostics;
using VXMusic;
using VXMusic.Last.fm;
using VXMusic.LogParser.Helpers;
using VXMusic.LogParser.VRChat;
using VXMusic.Recognition.AudD;
using VXMusic.Recognition.Shazam;
using VXMusic.Spotify;
using static VXMusic.LogParser.VRChat.VRChatLogParser;

namespace VXMusic
{
    class Program
    {
        public static void Main()
        {
            //VRChatLogParser.Run();

            var response = DoThing();
            Trace.WriteLine($"Grimes has {response.Result.PlayCount} listeners!");
        }

        public async static Task<LastAlbum> DoThing()
        {
            return await LastFmClient.InitialiseLastFmClient();
        }
    }
}