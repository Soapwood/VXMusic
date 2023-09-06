using VXMusic.Conversion;
using Microsoft.Extensions.Logging;

namespace VXMusic.Recognition.Shazam;

public class ShazamClient : IRecognitionClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShazamClient> _logger;

    private readonly ShazamHttpClient _shazamHttpClient;

    public ShazamClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<ShazamClient>)) 
            as ILogger<ShazamClient> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogInformation("Creating ShazamClient.");
        
        _shazamHttpClient = new ShazamHttpClient(); // TODO Make factgory for ShazamAPI client
    }

    public async Task<IRecognitionApiClientResponse> RunRecognition()
    {
        _logger.LogInformation("Running recognition using Shazam.");

        var converter = new AudioDataConverter(_serviceProvider);
        var shazamAudioData = converter.ConvertWavToBase64EncodedString();

        if(shazamAudioData == null)
        {
            _logger.LogInformation("Could not get suitable data for Shazam Recognition. Skipping recognition.");

            return new ShazamResponse()
            {
                Status = Status.RecordingError
            };
        } 
        else
        {
            _logger.LogInformation("Sending converted recorded data to shazam for Recognition.");
            return await _shazamHttpClient.GetArtist(shazamAudioData);
        }
    }
}