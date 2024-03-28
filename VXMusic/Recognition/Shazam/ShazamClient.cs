using VXMusic.Conversion;
using Microsoft.Extensions.Logging;

namespace VXMusic.Recognition.Shazam;

public class ShazamClient : IRecognitionClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShazamClient> _logger;

    private readonly ShazamHttpClient _shazamHttpClient;

    public bool IsUsingInjectedApiKey;
    
    private readonly string DefaultShazamApiKey = "bb058b1a1cmsh9026752692f380bp1ffca1jsnc94e79c37484";

    public ShazamClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<ShazamClient>)) 
            as ILogger<ShazamClient> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogInformation("Creating ShazamClient.");
        _logger.LogDebug("ShazamClient is configured to use DefaultShazamApiKey");

        _shazamHttpClient = new ShazamHttpClient(DefaultShazamApiKey); 
    }
    
    public ShazamClient(IServiceProvider serviceProvider, string apiKey)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<ShazamClient>)) 
            as ILogger<ShazamClient> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogInformation("Creating ShazamClient.");
        _logger.LogDebug("ShazamClient is configured to use an injected Api Key.");

        IsUsingInjectedApiKey = true;
        _shazamHttpClient = new ShazamHttpClient(apiKey); // TODO Make factgory for ShazamAPI client
    }

    public async Task<bool> SetByoApiKeyAndTest(string byoApikey)
    {
        _shazamHttpClient.SetApiKey(byoApikey);
        return await _shazamHttpClient.TestApiServiceConnection();
    }

    public void SetDefaultApiKey()
    {
        _shazamHttpClient.SetApiKey(DefaultShazamApiKey);
    }

    public async Task<bool> TestApiConnection()
    {
        return await _shazamHttpClient.TestApiServiceConnection();
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