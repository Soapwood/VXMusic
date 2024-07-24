using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Controls;
using Octokit;
using VXAutoUpdaterDesktop;
using System.Windows;
using Application = System.Windows.Application;
using FileMode = System.IO.FileMode;
using ProductHeaderValue = Octokit.ProductHeaderValue;

public class VXMusicAutoUpdater
{
    private readonly string _repositoryOwner;
    private readonly string _repositoryName;
    private readonly string _personalAccessToken;
    private readonly GitHubClient _gitHubClient;

    public readonly string AutoUpdaterAppDataPath;
    public static string OverlayAppDataPath;
    public string UpdateZipName;
    public string UpdateZipPath;

    public static List<string> ReleaseBranchOptions = new List<string>()
    {
        "Stable",
        "Nightly"
    };

    public VXMusicAutoUpdater(string repositoryOwner, string repositoryName, string personalAccessToken = "")
    {
        _repositoryOwner = repositoryOwner;
        _repositoryName = repositoryName;
        _personalAccessToken = personalAccessToken;

        AutoUpdaterAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions", "VXMusic", "AutoUpdater");
        OverlayAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions", "VXMusic", "Overlay");
        
        if (!Path.Exists(AutoUpdaterAppDataPath))
            Directory.CreateDirectory(AutoUpdaterAppDataPath);

        if (!string.IsNullOrEmpty(_personalAccessToken))
        {
            Console.WriteLine("Authenticaing using Personal Access Token");
            _gitHubClient = new GitHubClient(new ProductHeaderValue(repositoryName))
            {
                Credentials = new Credentials(_personalAccessToken)
            };
        }
        else
        {
            Console.WriteLine("No PAT specified. Will fetch from Public Endpoint");
            _gitHubClient = new GitHubClient(new ProductHeaderValue(repositoryName));
        }
    }
    
    private static void UpdateMessageInMainWindow(string message)
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.UpdateTextBlockMessage(message);
        }
    }
    
    private static void UpdateProgressBarInMainWindow(long progressPercent)
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.UpdateProgressBar(progressPercent);
        }
    }

    public static async Task UpdateApplicationBasedOnRequestedVersion(string branch, Release release, VXMusicAutoUpdater _autoUpdater)
    {
        UpdateMessageInMainWindow("Downloading update...");
        if (await _autoUpdater.DownloadUpdate(release))
        {
            UpdateMessageInMainWindow("Update downloaded successfully.");
            
            // Call the method to extract and replace files
            string extractPath = Path.Combine(_autoUpdater.AutoUpdaterAppDataPath, $"{_autoUpdater.UpdateZipName.Split(".zip")[0]}");
            string targetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "VXMusic");
            
            _autoUpdater.ExtractAndReplaceVxInstallation(_autoUpdater.UpdateZipPath, extractPath, targetPath);

            UpdateMessageInMainWindow($"Installation of [{branch}] ({release.Name}) was Successful!");
        }
        else
        {
            Console.WriteLine("Failed to download update.");
        }
    }

    public async Task<List<Release>> GetLatestVersionsForBranch(string branch)
    {
        try
        {
            var allReleases = await _gitHubClient.Repository.Release.GetAll(_repositoryOwner, _repositoryName);
            IEnumerable <Release> releasesToDisplay = null;

            if(string.Equals("Stable", branch))
                releasesToDisplay = allReleases.Where(release => !release.Prerelease && !release.Draft);
            else if(string.Equals("Nightly", branch))
                releasesToDisplay = allReleases.Where(release => release.Prerelease && !release.Draft );


            return releasesToDisplay.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking for updates: {ex.Message}");
        }

        // No new update available or error occurred
        return null;
    }

    public async Task<bool> DownloadUpdate(Release release)
    {
        try
        {
            var asset = release.Assets.FirstOrDefault(asset => string.Equals(asset.Name, $"VXMusic_{release.TagName}.zip"));
            if (asset != null)
            {
                UpdateZipName = asset.Name;
                UpdateZipPath = Path.Combine(AutoUpdaterAppDataPath, UpdateZipName);
                
                using (var httpClient = new HttpClient())
                {
                    if(!string.IsNullOrEmpty(_personalAccessToken))
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _personalAccessToken);
                    httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VXMusic", "1.0"));
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    
                    var response = await httpClient.GetAsync(asset.Url, HttpCompletionOption.ResponseHeadersRead);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var canReportProgress = totalBytes != -1;

                        using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                               fileStream = new FileStream(UpdateZipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            var buffer = new byte[8192];
                            int bytesRead;
                            long totalRead = 0;
                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalRead += bytesRead;
                                if (canReportProgress)
                                {
                                    UpdateMessageInMainWindow($"Download progress: {totalRead * 100 / totalBytes}%");
                                    UpdateProgressBarInMainWindow(totalRead * 100 / totalBytes);
                                }
                            }
                        }
                        UpdateMessageInMainWindow("Asset downloaded successfully.");

                        return true;
                    }
                    else
                    {
                        UpdateMessageInMainWindow($"Failed to download asset. Status code: {response.StatusCode}");
                    }
                }
            }
            else
            {
                UpdateMessageInMainWindow("No assets found in the latest release.");
            }
        }
        catch (Exception ex)
        {
            UpdateMessageInMainWindow($"Error downloading update: {ex.Message}");
        }

        // Update download failed
        return false;
    }

    public void ExtractAndReplaceVxInstallation(string zipPath, string extractPath, string targetPath)
    {
        // Ensure the extract directory exists and is empty
        if (Directory.Exists(extractPath))
        {
            Directory.Delete(extractPath, true);
        }
        
        Directory.CreateDirectory(extractPath);

        // Extract the ZIP file
        UpdateMessageInMainWindow("Extracting Release Zip...");

        try
        {
            ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        }
        catch (UnauthorizedAccessException uae)
        {
            Console.WriteLine("User does not have the required privileges, please run as Administrator");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured: {e}");
        }

        UpdateMessageInMainWindow("Replacing current installation...");
        // Replace the contents of the target directory
        
        try
        {
            UpdateMessageInMainWindow($"Installing new Release...");
            ReplaceDirectoryContents(extractPath, targetPath);
        }
        catch (UnauthorizedAccessException uae)
        {
            Console.WriteLine("User does not have the required privileges, please run as Administrator");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured: {e}");
        }
        
        // Copy Overlay to AppData
        try
        {
            UpdateMessageInMainWindow($"Installing Overlay...");
            ReplaceDirectoryContents(Path.Combine("..", extractPath, "Overlay"), OverlayAppDataPath);
        }
        catch (UnauthorizedAccessException uae)
        {
            Console.WriteLine("User does not have the required privileges, please run as Administrator");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured: {e}");
        }
        
        try
        {
            UpdateMessageInMainWindow("Deleting downloaded zip...");
            if(File.Exists(zipPath))
                File.Delete(zipPath);
            
            UpdateMessageInMainWindow("Deleting extracted zip folder...");
            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);
        }
        catch (UnauthorizedAccessException uae)
        {
            Console.WriteLine("User does not have the required privileges, please run as Administrator");
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occured: {e}");
        }
    }

    private void ReplaceDirectoryContents(string sourceDir, string targetDir)
    {
        // Delete existing files and directories in the target directory
        foreach (var file in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
            File.Delete(file);

        foreach (var dir in Directory.GetDirectories(targetDir))
            Directory.Delete(dir, true);

        // Copy new files and directories from the source directory to the target directory
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFileName = Path.Combine(targetDir, Path.GetFileName(file));
            File.Copy(file, destFileName);
        }
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destDirName = Path.Combine(targetDir, Path.GetFileName(dir));
            DirectoryCopy(dir, destDirName, true);
        }
    }

    private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory does not exist or could not be found: {sourceDirName}");
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destDirName);

        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, false);
        }

        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }
    
    static void ScheduleSelfDelete(string filePath)
    {
        ProcessStartInfo info = new ProcessStartInfo
        {
            Arguments = $"/C choice /C Y /N /D Y /T 3 & Del \"{filePath}\"",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = "cmd.exe"
        };
        Process.Start(info);
    }
}
