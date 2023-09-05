using Microsoft.Extensions.Logging;
using VXMusic.Conversion;

namespace VXMusic.Recognition.AudD;

public class AudDClient : IRecognitionClient
{
    private ILogger<AudDClient> _logger;

    private AudDHttpClient _audDHttpClient;
    
    public AudDClient(ILogger<AudDClient> logger)
    {
        _logger = logger;
        _logger.LogInformation("Creating AudDClient.");

        _audDHttpClient = new AudDHttpClient();
    }

    public async Task<IRecognitionApiClientResponse> RunRecognition()
    {
        _logger.LogInformation("Running recognition using AudD.");

        var converter = new AudioDataConverter(_logger);

        var audDAudioData = converter.ConvertWavToMp3Async();

        if (audDAudioData == null)
        {
            _logger.LogInformation("Could not get suitable data for AudD Recognition. Skipping recognition.");
            
            return new AudDResponse()
            {
                Status = Status.RecordingError
            };
        }
        else
        {
            _logger.LogInformation("Sending converted recorded data to AudD for Recognition.");
            return await _audDHttpClient.GetArtist(audDAudioData);
        } 
    }
}