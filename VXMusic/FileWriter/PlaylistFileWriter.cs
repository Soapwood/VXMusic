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

    public void AddLineToFileIfDateMatches(string currentWorldName, string textToAdd)
    {
        string fileName = GetCurrentFileName(currentWorldName);

        // Combine the file name with the VXMusic folder path
        string filePath = Path.Combine(_vxMusicFolder, fileName);

        // Check if the file exists and the date matches
        if (File.Exists(filePath))
        {
            _logger.LogTrace($"Playlist file already exists. Will add to discovered file: {filePath}");

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

    private string GetCurrentFileName(string currentWorldName)
    {
        string fileName = "";
        // Get the current date in MM-dd format
        string currentDate = DateTime.Now.ToString("dd-MM");
        
        // Generate the file name with MM-dd format and add location if exists
        if (currentWorldName != null)
        {
            fileName = $"{currentDate} {currentWorldName}.txt";
        }
        else
        {
            fileName = $"{currentDate}.txt";
        }

        return fileName;
    }
}