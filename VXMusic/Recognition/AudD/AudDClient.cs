using VXMusic.Conversion;

namespace VXMusic.Recognition.AudD;

public class AudDClient : IRecognitionClient
{
    private AudDHttpClient _audDHttpClient;
    
    public AudDClient()
    {
        _audDHttpClient = new AudDHttpClient();
    }

    public async Task<IApiClientResponse> RunRecognition()
    {
        var audDAudioData = await AudioDataConverter.ConvertWavToMp3Async();
        return await _audDHttpClient.GetArtist(audDAudioData);
    }
}