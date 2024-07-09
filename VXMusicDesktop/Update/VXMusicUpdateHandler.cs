using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Logging;
using Octokit;

namespace VXMusicDesktop.Update;

public class VXMusicUpdateHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VXMusicUpdateHandler> _logger;
    
    private readonly string _repositoryOwner = "Soapwood";
    private readonly string _repositoryName = "VXMusic";
    private readonly string _personalAccessToken = "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";
    private readonly GitHubClient _gitHubClient;
    
    public VXMusicUpdateHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService(typeof(ILogger<VXMusicUpdateHandler>)) 
            as ILogger<VXMusicUpdateHandler> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogTrace("Creating VXMusicUpdate.");
        
        if (!string.IsNullOrEmpty(_personalAccessToken))
        {
            _logger.LogInformation("Authenticaing using Personal Access Token");
            _gitHubClient = new GitHubClient(new ProductHeaderValue(_repositoryName))
            {
                Credentials = new Credentials(_personalAccessToken)
            };
        }
        else
        {
            _logger.LogInformation("No PAT specified. Will fetch from Public Endpoint");
            _gitHubClient = new GitHubClient(new ProductHeaderValue(_repositoryName));
        }
    }
    
    public async Task<bool> IsVxMusicUpdateAvailable()
    {
        try
        {
            // Get the latest release from GitHub
            var releases = await _gitHubClient.Repository.Release.GetAll(_repositoryOwner, _repositoryName);

            var currentApplicationVersion = new Version($"{App.ApplicationVersion.ToString()}");
            var latestRelease = releases.FirstOrDefault();
            var latestReleaseVersion = new Version($"{latestRelease.TagName}");

            _logger.LogTrace($"Current Version: {currentApplicationVersion}. Latest Release Version: {latestReleaseVersion}");

            if (currentApplicationVersion == latestReleaseVersion)
            {
                _logger.LogInformation("No new updates available.");
                return false;
            }

            if (latestReleaseVersion > currentApplicationVersion)
            {
                _logger.LogInformation("A new update was found.");
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking for updates: {ex.Message}");
        }
        return false;
    }

    public void LaunchVxMusicUpdater()
    {
        // Prepare AppData folder for temporary copy of VXAutoUpdater
        string autoUpdaterPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VXMusic", "VXAutoUpdater");
        string temporaryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VXMusic", "AutoUpdaterTemp");
        
        CopyDirectory(autoUpdaterPath, temporaryFolder);
        
        string executablePath = Path.Combine(temporaryFolder, "VXAutoUpdater.exe");
                
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false
            };
            
            Process process = Process.Start(startInfo);
            
            if (process != null)
            {
                _logger.LogInformation($"Started process {process.ProcessName} with ID {process.Id}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to VXMusicUpdater: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void CopyDirectory(string sourceDir, string destDir)
    {
        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        foreach (string filePath in Directory.GetFiles(sourceDir))
        {
            string destFilePath = Path.Combine(destDir, Path.GetFileName(filePath));
            File.Copy(filePath, destFilePath, true);
        }

        foreach (string directoryPath in Directory.GetDirectories(sourceDir))
        {
            string destDirPath = Path.Combine(destDir, Path.GetFileName(directoryPath));
            CopyDirectory(directoryPath, destDirPath);
        }
    }
}