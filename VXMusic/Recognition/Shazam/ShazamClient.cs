using VXMusic.Conversion;
using Microsoft.Extensions.Logging;

namespace VXMusic.Recognition.Shazam;

public class ShazamClient : IRecognitionClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShazamClient> _logger;

    private readonly ShazamHttpClient _shazamHttpClient;

    private readonly string DefaultShazamApiKey = "bb058b1a1cmsh9026752692f380bp1ffca1jsnc94e79c37484";
    private string ShazamByoApiKey = "";
    
    public ShazamClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<ShazamClient>)) 
            as ILogger<ShazamClient> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogInformation("Creating ShazamClient.");

        
        var apiKey = String.IsNullOrEmpty(ShazamByoApiKey) ? DefaultShazamApiKey : ShazamByoApiKey;
        
        _shazamHttpClient = new ShazamHttpClient(apiKey); // TODO Make factgory for ShazamAPI client
    }

    public void SetByoApiKey(string byoApikey)
    {
        ShazamByoApiKey = byoApikey;
        // TODO Save the key to user settings
        
        _shazamHttpClient.SetShazamApiKey(ShazamByoApiKey);
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