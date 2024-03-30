using Microsoft.Extensions.Logging;
using VXMusic.Conversion;

namespace VXMusic.Recognition.AudD;

public class AudDClient : IRecognitionClient
{
    private readonly IServiceProvider _serviceProvider;
    private ILogger<AudDClient> _logger;

    private AudDHttpClient _audDHttpClient;

    private readonly string DefaultAudDApiKey = "f0c51b25ecd8068fce82a5bcfd9f5b6e";
    
    public AudDClient(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = _serviceProvider.GetService(typeof(ILogger<AudDClient>)) 
            as ILogger<AudDClient> ?? throw new ApplicationException("A logger must be created in service provider.");
        
        _logger.LogTrace("Creating AudDClient.");

        _audDHttpClient = new AudDHttpClient();
    }

    public async Task<IRecognitionApiClientResponse> RunRecognition()
    {
        _logger.LogInformation("Running recognition using AudD.");

        var converter = new AudioDataConverter(_serviceProvider);

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

    public async Task<bool> SetByoApiKeyAndTest(string byoApiKey)
    {
        _audDHttpClient.SetApiKey(byoApiKey);
        return await _audDHttpClient.TestConnection();
    }

    public void SetDefaultApiKey()
    {
        _audDHttpClient.SetApiKey(DefaultAudDApiKey);
    }

    public async Task<bool> TestApiConnection()
    {
        return await _audDHttpClient.TestConnection();
    }
}