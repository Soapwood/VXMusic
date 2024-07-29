using NAudio.Lame;
using NAudio.Wave;
using Microsoft.Extensions.Logging;

namespace VXMusic.Conversion;

public class AudioDataConverter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AudioDataConverter> _logger;

    public AudioDataConverter(IServiceProvider serviceProvider) 
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<AudioDataConverter>)) 
            as ILogger<AudioDataConverter> ?? throw new ApplicationException("A logger must be created in service provider.");
    }

    public byte[]? ConvertWavToMp3Async() // TODO Inject bytes not file 
    {
        _logger.LogTrace("Converting WAV to MP3.");

        byte[] audioData = File.ReadAllBytes(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions", "VXMusic", "Cache", "output.wav")); 

        if (audioData == null)
        {
            _logger.Log(LogLevel.Information,"Could not read any bytes from output.wav, skipping conversion.");
            return null;
        }
        else
        {
            _logger.Log(LogLevel.Information,"Audio recording read succeeded. Converting to MP3");

            using (var retMs = new MemoryStream())
            using (var memoryStream = new MemoryStream(audioData))
            using (var waveFileReader = new WaveFileReader(memoryStream))
            using (var mp3FileWriter = new LameMP3FileWriter(retMs, waveFileReader.WaveFormat, 128))
            {
                waveFileReader.CopyTo(mp3FileWriter);
                return retMs.ToArray();
            }
        }
    }
    
    public string? ConvertWavToBase64EncodedString() 
    {
        _logger.LogTrace("Converting WAV to Base64 encoded string.");

        var audioCacheFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VirtualXtensions", "VXMusic", "Cache", "output.wav");
        
        byte[] audioData = File.ReadAllBytes(audioCacheFile);
        
        if(audioData == null)
        {
            _logger.LogTrace("Could not read any bytes from output.wav, skipping conversion.");
            return null;
        }
        else
        {
            _logger.LogTrace("Audio recording read succeeded. Converting to Base64 String.");
            return Convert.ToBase64String(audioData);
        }
    }
}