using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VXMusicDesktop.Core;

public class SteamVROverlayAppsInterface
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SteamVROverlayAppsInterface> _logger;
    
    private static readonly string SteamAppConfigPath = "C:\\Program Files\\Steam\\config\\appconfig.json";
    private static readonly string DefaultVxMusicManifestPath = "C:\\Program Files\\VXMusic\\manifest.vrmanifest";

    public SteamVROverlayAppsInterface(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService(typeof(ILogger<SteamVROverlayAppsInterface>)) 
            as ILogger<SteamVROverlayAppsInterface> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogTrace("Creating SteamVROverlayAppsInterface.");
    }
    
    public bool InstallVxMusicAsSteamVrOverlay()
    {
        _logger.LogInformation("Installing VXMusic as a SteamVR Overlay.");
        return AddManifestEntryToAppConfig(DefaultVxMusicManifestPath);
    }
    
    public bool IsManifestEntryInAppConfig()
    {
        _logger.LogDebug("Checking if VXMusic is already added as a SteamVR Overlay..");
        var (manifestPaths, _) = ReadContentsOfAppConfig();
        return SteamVrManifestPathExistsInAppConfig(manifestPaths, SteamAppConfigPath);
    }

    private bool AddManifestEntryToAppConfig(string vxMusicManifestFile)
    {
        try
        {
            var (manifestPaths, jsonObj) = ReadContentsOfAppConfig();
            
            // Check if the entry already exists
            if (SteamVrManifestPathExistsInAppConfig(manifestPaths, vxMusicManifestFile))
            {
                _logger.LogDebug("Manifest path already exists in config file. Exiting.");
                return true;
            }

            // Add a new string to the manifest_paths array
            manifestPaths.Add(vxMusicManifestFile);

            // Convert the updated JSON object back to a string
            string updatedJsonContent = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

            // Write the updated JSON content back to the file
            File.WriteAllText(SteamAppConfigPath, updatedJsonContent);
            _logger.LogTrace($"New path {SteamAppConfigPath} added successfully.");
            
            return true;
        }
        catch (FileNotFoundException)
        {
            _logger.LogError($"Error: The file '{SteamAppConfigPath}' was not found.");
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError($"Error: Failed to parse JSON content. Details: {jsonEx.Message}");
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogError("Error: You do not have permission to read or write to this file.");
        }
        catch (IOException ioEx)
        {
            _logger.LogError($"Error: An I/O error occurred. Details: {ioEx.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An unexpected error occurred. Details: {ex.Message}");
        }

        return false;
    }

    private (JArray, JObject) ReadContentsOfAppConfig()
    {
        string jsonContent = File.ReadAllText(SteamAppConfigPath);

        var jsonObj = JsonConvert.DeserializeObject<JObject>(jsonContent);

        JArray manifestPaths = (JArray) jsonObj["manifest_paths"];

        if (manifestPaths == null)
        {
            throw new Exception($"manifest_paths array not found in {SteamAppConfigPath}.");
        }
        
        return (manifestPaths, jsonObj);
    }
    
    private bool SteamVrManifestPathExistsInAppConfig(JArray manifestPaths, string pathToCheck)
    {
        foreach (var path in manifestPaths)
        {
            if (path.ToString().Equals(DefaultVxMusicManifestPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("VXMusic is already added as a SteamVR Overlay.");
                return true;
            }
        }
        
        _logger.LogWarning("VXMusic is not added as a SteamVR Overlay.");
        return false;
    }
}
