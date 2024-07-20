using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace VXMusicDesktop.Update;

public class VXMusicSettingsSyncHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VXMusicSettingsSyncHandler> _logger;
    
    private static readonly string UserSettingsFileName = "user.config";
    private static readonly string UrlFolderPattern = "VXMusicDesktop_Url_*";

    public string CurrentUserSettingsFile;
    public bool CurrentUserSettingsFileExists => File.Exists(CurrentUserSettingsFile);

    public VXMusicSettingsSyncHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService(typeof(ILogger<VXMusicSettingsSyncHandler>)) 
            as ILogger<VXMusicSettingsSyncHandler> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogTrace("Creating VXMusicSettingsSyncHandler.");
        
        CurrentUserSettingsFile = GetCurrentConfigPath();
        
        if (CurrentUserSettingsFileExists)
        {
            _logger.LogInformation($"Current User Settings File: {CurrentUserSettingsFile}");
        }
        else
        {
            _logger.LogTrace($"Current User Settings File: {CurrentUserSettingsFile} doesn't exist.");
            MigrateSettings();
        }
    }

    public void MigrateSettings()
    {
        string latestConfigPath = GetLatestConfigPath();
        if (latestConfigPath != null)
        {
            _logger.LogInformation($"Found previous user settings: {latestConfigPath}. Migrating to: {CurrentUserSettingsFile}");
            EnsureAppDataUserSettingsFolderCreated();
            File.Copy(latestConfigPath, CurrentUserSettingsFile, true);
        }
        else
        {
            _logger.LogInformation($"No Previous User Settings Found. Creating User Settings file in {CurrentUserSettingsFile}");
            EnsureAppDataUserSettingsFolderCreated();
            VXUserSettings.Settings.SetHasLaunched(true);
        }
    }
    
    private string GetCurrentConfigPath()
    {
        return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
    }
    
    private string GetLatestConfigPath()
    {
        string localAppDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VirtualXtensions");
        
        var versionedFolders = new List<Tuple<Version, string>>();

        if (Directory.Exists(localAppDataDir))
        {
            var urlFolders = Directory.GetDirectories(localAppDataDir, UrlFolderPattern);

            foreach (var urlFolder in urlFolders)
            {
                var versionFolders = Directory.GetDirectories(urlFolder)
                    .Select(folder => new
                    {
                        Path = folder,
                        Version = ParseVersion(Path.GetFileName(folder))
                    })
                    .Where(x => x.Version != null)
                    .Select(x => Tuple.Create(x.Version, x.Path));

                versionedFolders.AddRange(versionFolders);
            }
        }

        var latestPreviousVersionFolder = versionedFolders
            .Where(v => v.Item1 < App.ApplicationVersion)
            .OrderByDescending(v => v.Item1)
            .FirstOrDefault();

        if (latestPreviousVersionFolder != null)
        {
            string configPath = Path.Combine(latestPreviousVersionFolder.Item2, UserSettingsFileName);
            if (File.Exists(configPath))
            {
                return configPath;
            }
        }

        return null;
    }
    
    private void EnsureAppDataUserSettingsFolderCreated()
    {
        var currentSettingsDirectoryPath = Path.GetDirectoryName(CurrentUserSettingsFile);
        
        if (!Directory.Exists(currentSettingsDirectoryPath))
            Directory.CreateDirectory(currentSettingsDirectoryPath);
    }

    private static Version ParseVersion(string versionString)
    {
        Version version;
        return Version.TryParse(versionString, out version) ? version : null;
    }
}