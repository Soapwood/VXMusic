using VXMusic.Conversion;
using Microsoft.Extensions.Logging;
using VXMusic.Recognition.Shazam.Keyless;

namespace VXMusic.Recognition.Shazam;

public class ShazamClient : IRecognitionClient
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ShazamClient> _logger;

    private readonly IHttpClient _httpClient;
    private readonly bool _useKeylessMode;

    public bool IsUsingInjectedApiKey { get; private set; }
    public bool IsUsingKeylessMode => _useKeylessMode;
    
    public ShazamClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<ShazamClient>)) 
            as ILogger<ShazamClient> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogInformation("Creating ShazamClient in keyless mode.");
        _logger.LogDebug("ShazamClient is configured to use keyless Apple ShazamKit approach");

        _useKeylessMode = true;
        IsUsingInjectedApiKey = false;
        _httpClient = new ShazamKeylessHttpClient(_logger); 
    }
    
    public ShazamClient(IServiceProvider serviceProvider, string apiKey)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<ShazamClient>)) 
            as ILogger<ShazamClient> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogInformation("Creating ShazamClient in legacy API key mode.");
        _logger.LogDebug("ShazamClient is configured to use an injected Api Key.");

        _useKeylessMode = false;
        IsUsingInjectedApiKey = true;
        _httpClient = new ShazamHttpClient(apiKey); // TODO Make factory for ShazamAPI client
    }

    public async Task<IRecognitionApiClientResponse> RunRecognition()
    {
        var mode = _useKeylessMode ? "keyless" : "legacy API key";
        _logger.LogInformation($"Running recognition using Shazam ({mode} mode).");

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
            return await _httpClient.GetArtist(shazamAudioData);
        }
    }

    public Task<bool> TestApiConnection()
    {
        // TODO Have meaningful test here
        throw new NotImplementedException();
    }
}