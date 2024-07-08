using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Octokit;

namespace VXMusicDesktop.Update;

public class VXMusicUpdate
{
    private readonly string _repositoryOwner;
    private readonly string _repositoryName;
    private readonly string _personalAccessToken;
    private readonly GitHubClient _gitHubClient;
    
    public VXMusicUpdate(string repositoryOwner, string repositoryName, string personalAccessToken = "")
    {
        _repositoryOwner = repositoryOwner;
        _repositoryName = repositoryName;
        _personalAccessToken = personalAccessToken;

        if (!string.IsNullOrEmpty(_personalAccessToken))
        {
            //Console.WriteLine("Authenticaing using Personal Access Token");
            _gitHubClient = new GitHubClient(new ProductHeaderValue(repositoryName))
            {
                Credentials = new Credentials(_personalAccessToken)
            };
        }
        else
        {
            //Console.WriteLine("No PAT specified. Will fetch from Public Endpoint");
            _gitHubClient = new GitHubClient(new ProductHeaderValue(repositoryName));
        }
    }
    
    public async Task<bool> CheckForUpdates(string currentVersion)
    {
        try
        {
            // Get the latest release from GitHub
            var releases = await _gitHubClient.Repository.Release.GetAll(_repositoryOwner, _repositoryName);
            var latestRelease = releases.FirstOrDefault();
            
            // TODO Compare against current version

            if (latestRelease != null && latestRelease.TagName != currentVersion)
            {
                // New version available, return true to indicate update available
                return true;
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"Error checking for updates: {ex.Message}");
        }

        // No new update available or error occurred
        return false;
    }

    public static async Task LaunchVXMusicUpdater()
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
                // Optionally, you can set other properties like Arguments, WorkingDirectory, etc.
            };
            
            Process process = Process.Start(startInfo);
            
            if (process != null)
            {
                //Console.WriteLine($"Started process {process.ProcessName} with ID {process.Id}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to start new application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
                
        //Environment.Exit(0); // Exit the current application
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