using NAudio.Lame;
using NAudio.Wave;
using Microsoft.Extensions.Logging;

namespace VXMusic.Conversion;

public class AudioDataConverter
{
    private ILogger _logger;

    public AudioDataConverter(ILogger logger) 
    {
        _logger = logger;
    }

    public byte[]? ConvertWavToMp3Async() // TODO Inject bytes not file 
    {
        _logger.Log(LogLevel.Information,"Converting WAV to MP3.");

        byte[] audioData = File.ReadAllBytes("output.wav"); // TODO Really need to fucking use just bytes.

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
        _logger.Log(LogLevel.Information,"Converting WAV to Base64 encoded string.");

        byte[] audioData = File.ReadAllBytes("output.wav"); // TODO Really need to fucking use just bytes.
        
        if(audioData == null)
        {
            _logger.Log(LogLevel.Information,"Could not read any bytes from output.wav, skipping conversion.");
            return null;
        }
        else
        {
            _logger.Log(LogLevel.Information,"Audio recording read succeeded. Converting to Base64 String.");
            return Convert.ToBase64String(audioData);
        }
    }
}