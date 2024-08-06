using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VXMusicDesktop.Core;

public class RegistryInterface
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RegistryInterface> _logger;
    private static List<string> _deferredLogMessages = new List<string>();
    
    private static readonly string VxMusicInstallPathRootKey = "{{VXMUSIC_INSTALL_PATH_ROOT}}";
    
    private static readonly string SteamRegistryValue = "SteamPath";
    private static readonly string SteamRegistryKey = @"HKEY_CURRENT_USER\Software\Valve\Steam";
    
    private const string VX_MUSIC_REGISTRY_VALUE = "InstallPath";
    private const string VX_MUSIC_DESKTOP_EXECUTABLE_REGISTRY_VALUE = "Executable";
    private const string VX_MUSIC_REGISTRY_KEY = @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\VirtualXtensions\VXMusic";
    
    public static string SteamVrAppConfigPath;
    public static string SteamInstallPath;
    
    public static string? VxMusicVrManifestConfigPath;
    public static string? VxMusicInstallPath;
    public static string? VxMusicDesktopExecutablePath;

    public static string? VxMusicAppDataDir;
    public static string? VxMusicVrManiFestAppDataDestinationPath;
    
    public RegistryInterface(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService(typeof(ILogger<RegistryInterface>)) 
            as ILogger<RegistryInterface> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogTrace("Creating SteamVROverlayAppsInterface.");

        FlushDeferredLogMessages();
    }

    public static void RunRegistryCheckOnStartup()
    {
        SteamInstallPath = GetRegistryEntry(SteamRegistryKey, SteamRegistryValue);
        _deferredLogMessages.Add($"Found SteamVR appconfig.json path: {SteamInstallPath}");
        
        VxMusicInstallPath = GetRegistryEntry(VX_MUSIC_REGISTRY_KEY, VX_MUSIC_REGISTRY_VALUE);
        _deferredLogMessages.Add($"Found VXMusic Install path: {VxMusicInstallPath}");

        VxMusicDesktopExecutablePath = GetRegistryEntry(VX_MUSIC_REGISTRY_KEY, VX_MUSIC_DESKTOP_EXECUTABLE_REGISTRY_VALUE);
        _deferredLogMessages.Add($"Found VXMusicDesktop executable path: {VxMusicDesktopExecutablePath}");

        VxMusicAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VirtualXtensions", "VXMusic");
        
        if (!string.IsNullOrEmpty(SteamInstallPath) && Directory.Exists(SteamInstallPath))
        {
            SteamVrAppConfigPath = Path.GetFullPath(Path.Combine(SteamInstallPath, "config", "appconfig.json"));
            _deferredLogMessages.Add($"Found SteamVR appconfig.json path: {SteamVrAppConfigPath}");
        }
        else
        {
            _deferredLogMessages.Add($"DID NOT FIND SteamVR appconfig.json path: {SteamVrAppConfigPath}");
        }
        
        if (!string.IsNullOrEmpty(VxMusicInstallPath) && Directory.Exists(VxMusicInstallPath))
        {
            VxMusicVrManifestConfigPath = Path.GetFullPath(Path.Combine(VxMusicAppDataDir, "manifest.vrmanifest"));
            _deferredLogMessages.Add($"Found VXMusic VR Manifest path: {VxMusicInstallPath}\\manifest.vrmanifest");
        }
        else
        {
            _deferredLogMessages.Add($"DID NOT FIND VXMusic VR Manifest path: {VxMusicInstallPath}");
        }
        
        VxMusicVrManiFestAppDataDestinationPath = Path.Combine(Path.Combine(RegistryInterface.VxMusicAppDataDir), "manifest.vrmanifest");
        
        // Injecting installation path into vrmanifest as soon as possible
        if (!File.Exists(VxMusicVrManiFestAppDataDestinationPath))
        {
            var formattedVxMusicDesktopPath = RegistryInterface.VxMusicDesktopExecutablePath.Replace(@"\", @"\\");
            var vrManifestJson = SteamVROverlayAppsInterface.VrManifestFileTemplate.Replace("{{VXMUSIC_INSTALL_PATH_ROOT}}", formattedVxMusicDesktopPath);
            
            _deferredLogMessages.Add($"Creating VR Manifest in AppData -> {VxMusicVrManiFestAppDataDestinationPath}");
            _deferredLogMessages.Add($"{vrManifestJson}");

            File.WriteAllText(VxMusicVrManiFestAppDataDestinationPath, vrManifestJson);
        }
    }
    
    private static void InjectVxMusicDesktopInstallPathIntoVrManifest(string manifestPath, string vxMusicDesktopInstallPath)
    {
        _deferredLogMessages.Add("Injecting VXMusicDesktop installation directory into VR Manifest");
        _deferredLogMessages.Add($"{vxMusicDesktopInstallPath} -> {manifestPath}");
        
        // Read the JSON file
        string jsonString = File.ReadAllText(manifestPath);
        
        // Parse the JSON string
        var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
        
        // Update the desired value
        var applications = jsonObject["applications"] as JArray;
        if (applications != null)
        {
            foreach (var app in applications)
            {
                if (app["app_key"] != null && app["app_key"].ToString() == "virtualxtensions.VXMusic")
                {
                    app["binary_path_windows"] = vxMusicDesktopInstallPath;
                }
            }
        }
        else
        {
            throw new Exception("Applications array not found in the JSON data.");
        }
        
        // Serialize the updated object
        string updatedJsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        
        // Write the JSON string back to the file
        File.WriteAllText(manifestPath, updatedJsonString);
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
            _deferredLogMessages.Add($"Error accessing registry entry {registryKey}: {registryValue}.");
            _deferredLogMessages.Add(ex.Message);
        }

        return "";
    }
    
    private void FlushDeferredLogMessages()
    {
        foreach (var message in _deferredLogMessages)
        {
            _logger.LogDebug(message);
        }
        _deferredLogMessages.Clear();
    }
    
}