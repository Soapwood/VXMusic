using Microsoft.Extensions.Logging;
using VXMusic.Branding;
using VXMusic.Lastfm.Scrobbling;

namespace VXMusic.FileWriter;

public class PlaylistFileWriter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PlaylistFileWriter> _logger;
    
    private string _vxMusicFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VXMusic");
    
    public PlaylistFileWriter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<PlaylistFileWriter>)) 
            as ILogger<PlaylistFileWriter> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        // Create the "VXMusic" folder if it doesn't exist
        if (!Directory.Exists(_vxMusicFolder))
        {
            _logger.LogTrace("VXMusic folder doesn't exist in My Documents. Creating now.");
            Directory.CreateDirectory(_vxMusicFolder);
        }
    }

    private string GetPlaylistFileHeader()
    {
        return ConsoleOutputBranding.VxMusicLogo +
               Environment.NewLine +
               "________________________________________________________________________________________" +
               Environment.NewLine + Environment.NewLine;
    }

    public void AddTrackEntryToPlaylistFile(string fileName, string textToAdd)
    {
        // Combine the file name with the VXMusic folder path
        string filePath = Path.Combine(_vxMusicFolder, fileName + ".txt");

        // Check if the file exists and the date matches
        if (File.Exists(filePath))
        {
            _logger.LogTrace($"Playlist file already exists. Will add to discovered file: {filePath}");

            if (IsTrackAlreadyInPlaylist(filePath, textToAdd))
            {
                _logger.LogTrace($"Track was already found in Playlist file. Will skip add.");
                return;
            }

            // Append the text to the existing file
            File.AppendAllText(filePath, textToAdd + Environment.NewLine);
        }
        else
        {
            _logger.LogTrace($"Creating new playlist file {filePath}");

            // Create a new file with the current date as the name
            File.WriteAllText(filePath, GetPlaylistFileHeader());
            File.AppendAllText(filePath, textToAdd + Environment.NewLine);
        }
    }

    private bool IsTrackAlreadyInPlaylist(string filePath, string trackLine)
    {
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            return Array.Exists(lines, line => line == trackLine);
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Could not read Playlist track file: {ex.Message}");
            return false;
        }
    }
}