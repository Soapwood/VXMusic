using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using SharpDX;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using System.Diagnostics;
using VXMusic;
using VXMusic.Lastfm;
using VXMusic.Lastfm.Authentication;
using VXMusic.Lastfm.Scrobbling;
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
        public async static Task Main()
        {
            //var last = new Lastfm.LastfmScrobbler();
            //var responseTask = await last.Login("", "");

            //var response = await last.Scrobble("Alcest", "Kodama", "Kodama");
            //response.Success
            //Trace.WriteLine("And now back in the main method");

            //Trace.WriteLine("And now back in the main method");
            //Trace.WriteLine($"Grimes has {response.Result.PlayCount} listeners!");
        }
    }
}