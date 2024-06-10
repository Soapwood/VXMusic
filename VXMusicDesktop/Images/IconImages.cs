using System;
using System.IO;

namespace VXMusicDesktop.Images;

public class IconImages
{
    public static string SpotifyLogo = File.ReadAllText(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay", "Images", "SpotifyIconBase64.txt"));
    public static string LastFmLogo = File.ReadAllText(
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Overlay", "Images", "LastFmIconBase64.txt"));
}