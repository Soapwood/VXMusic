using System;
using System.Linq;
using System.Threading.Tasks;
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
}