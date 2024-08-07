using System;
using Microsoft.Win32;

namespace VXMusicDesktop.Core;

public class RegistryInterface
{
    private static readonly string VxMusicInstallPathRootKey = "{{VXMUSIC_INSTALL_PATH_ROOT}}";
    
    private static readonly string SteamRegistryValue = "SteamPath";
    private static readonly string SteamRegistryKey = @"HKEY_CURRENT_USER\Software\Valve\Steam";
    
    private const string VX_MUSIC_REGISTRY_VALUE = "InstallPath";
    private const string VX_MUSIC_DESKTOP_EXECUTABLE_REGISTRY_VALUE = "Executable";
    private const string VX_MUSIC_REGISTRY_KEY = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\VirtualXtensions\VXMusic";

    public static string? VxMusicInstallPath;
    public static string? VxMusicDesktopExecutablePath;

    public static void GetExistingVXMusicInstallationPath()
    {
        VxMusicInstallPath = GetRegistryEntry(VX_MUSIC_REGISTRY_KEY, VX_MUSIC_REGISTRY_VALUE);
        Console.WriteLine($"VxMusicInstallPath: {VxMusicInstallPath}");
        
        VxMusicDesktopExecutablePath = GetRegistryEntry(VX_MUSIC_REGISTRY_KEY, VX_MUSIC_DESKTOP_EXECUTABLE_REGISTRY_VALUE);
        Console.WriteLine($"VxMusicDesktopExecutablePath: {VxMusicDesktopExecutablePath}");
    }

    private static string GetRegistryEntry(string registryKey, string registryValue)
    {
        try
        {
            // Try to read the Steam installation path from the registry
            object value = Registry.GetValue(registryKey, registryValue, null);
            if (value != null) return value.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing registry entry {registryKey}: {registryValue}.");
            Console.WriteLine(ex.Message);
        }

        return "";
    }
}