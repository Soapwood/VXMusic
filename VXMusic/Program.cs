using SharpDX;
using SpotifyAPI.Web;
using VXMusic;
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
            VRChatLogParser.Run();
        }
    }
}