using System;
using System.IO;

namespace VXMusic.Overlay
{
    class Utils
    {
        public static string PathToResource(string file)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay/Resources", file);
        }
    }
}
