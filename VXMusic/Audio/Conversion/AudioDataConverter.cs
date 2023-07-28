using NAudio.Lame;
using NAudio.Wave;

namespace VXMusic.Conversion;

public class AudioDataConverter
{
    public static async Task<byte[]> ConvertWavToMp3Async() // TODO Inject bytes not file 
    {
        byte[] audioData = File.ReadAllBytes("output.wav"); // TODO Really need to fucking use just bytes.
        
        using(var retMs = new MemoryStream())
        using (var memoryStream = new MemoryStream(audioData))
        using(var waveFileReader = new WaveFileReader(memoryStream))
        using (var mp3FileWriter = new LameMP3FileWriter(retMs, waveFileReader.WaveFormat, 128))
        {
            waveFileReader.CopyTo(mp3FileWriter);
            return retMs.ToArray();
        }
    }
    
    public static async Task<string> ConvertWavToBase64EncodedString() 
    {
        byte[] audioData = File.ReadAllBytes("output.wav"); // TODO Really need to fucking use just bytes.
        return Convert.ToBase64String(audioData);
    }
}