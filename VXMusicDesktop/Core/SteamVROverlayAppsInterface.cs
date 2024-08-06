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

    public bool SteamAppConfigPathExists => Directory.Exists(RegistryInterface.SteamVrAppConfigPath);
    public bool SteamInstallPathExists => Directory.Exists(RegistryInterface.SteamInstallPath);
    
    public static readonly string VrManifestFileTemplate = @"
{
	""source"" : ""builtin"",
	""applications"": [{
	""app_key"": ""virtualxtensions.VXMusic"",
	""launch_type"": ""binary"",
	""binary_path_windows"": ""{{VXMUSIC_INSTALL_PATH_ROOT}}"",
	""is_dashboard_overlay"": true,

	""strings"": {
	    ""en_us"": {
			""name"": ""VXMusic"",
			""description"": ""VXMusic""
		}
	}
	}]
}";

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
        return AddManifestEntryToAppConfig();
    }

    public bool IsManifestEntryInAppConfig()
    {
        _logger.LogDebug("Checking if VXMusic is already added as a SteamVR Overlay..");

        if (!SteamAppConfigPathExists)
            return false;
        
        var (manifestPaths, _) = ReadContentsOfAppConfig();
        return SteamVrManifestPathExistsInAppConfig(manifestPaths, RegistryInterface.SteamVrAppConfigPath);
    }

    private bool AddManifestEntryToAppConfig()
    {
        try
        {
            var (manifestPaths, jsonObj) = ReadContentsOfAppConfig();
            
            // Check if the entry already exists
            if (SteamVrManifestPathExistsInAppConfig(manifestPaths, RegistryInterface.SteamVrAppConfigPath))
            {
                _logger.LogDebug("Manifest path already exists in config file. Exiting.");
                return true;
            }

            // Add a new string to the manifest_paths array
            manifestPaths.Add(RegistryInterface.VxMusicVrManifestConfigPath);

            // Convert the updated JSON object back to a string
            string updatedJsonContent = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);

            // Write the updated JSON content back to the file
            File.WriteAllText(RegistryInterface.SteamVrAppConfigPath, updatedJsonContent);
            _logger.LogTrace($"New path {RegistryInterface.SteamVrAppConfigPath} added successfully.");
            
            return true;
        }
        catch (FileNotFoundException)
        {
            _logger.LogError($"Error: The file '{RegistryInterface.SteamVrAppConfigPath}' was not found.");
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
        string jsonContent = File.ReadAllText(RegistryInterface.SteamVrAppConfigPath);

        var jsonObj = JsonConvert.DeserializeObject<JObject>(jsonContent);

        JArray manifestPaths = (JArray) jsonObj["manifest_paths"];

        if (manifestPaths == null)
        {
            throw new Exception($"manifest_paths array not found in {RegistryInterface.SteamVrAppConfigPath}.");
        }
        
        return (manifestPaths, jsonObj);
    }
    
    private bool SteamVrManifestPathExistsInAppConfig(JArray manifestPaths, string pathToCheck)
    {
        foreach (var path in manifestPaths)
        {
            if (path.ToString().Equals(RegistryInterface.VxMusicVrManifestConfigPath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("VXMusic is already added as a SteamVR Overlay.");
                return true;
            }
        }
        
        _logger.LogWarning("VXMusic is not added as a SteamVR Overlay.");
        return false;
    }
}
