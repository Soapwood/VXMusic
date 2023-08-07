using VXMusic.Conversion;

namespace VXMusic.Recognition.Shazam;

public class ShazamClient : IRecognitionClient
{
    private ShazamHttpClient _shazamHttpClient;
    
    public ShazamClient()
    {
        _shazamHttpClient = new ShazamHttpClient(); // TODO Make factgory for ShazamAPI client
    }

    public async Task<IApiClientResponse> RunRecognition()
    {
        var shazamAudioData = await AudioDataConverter.ConvertWavToBase64EncodedString();
        return await _shazamHttpClient.GetArtist(shazamAudioData);
    }
}