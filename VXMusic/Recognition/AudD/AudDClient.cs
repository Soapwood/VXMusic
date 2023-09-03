using NLog;
using VXMusic.Conversion;

namespace VXMusic.Recognition.AudD;

public class AudDClient : IRecognitionClient
{
    private ILogger _logger;

    private AudDHttpClient _audDHttpClient;
    
    public AudDClient(ILogger logger)
    {
        _logger = logger;
        _audDHttpClient = new AudDHttpClient();
    }

    public async Task<IRecognitionApiClientResponse> RunRecognition()
    {
        var converter = new AudioDataConverter(_logger);

        var audDAudioData = converter.ConvertWavToMp3Async();

        if (audDAudioData == null)
        {
            _logger.Info("Could not get suitable data for AudD Recognition. Skipping recognition.");

            return new AudDResponse()
            {
                Status = Status.RecordingError
            };
        }
        else
        {
            _logger.Info("Sending converted recorded data to AudD for Recognition.");
            return await _audDHttpClient.GetArtist(audDAudioData);
        } 
    }
}