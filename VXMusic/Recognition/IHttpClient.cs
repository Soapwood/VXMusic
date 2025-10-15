using Disco.Business.Interfaces.Dtos.AudD;
using Disco.Integration.Interfaces.Dtos.AudD;

namespace VXMusic;

public interface IHttpClient
{
    public Task<IRecognitionApiClientResponse> GetArtist(byte[] audioBytes);
    public Task<IRecognitionApiClientResponse> GetArtist(string audioDataBase64);
    public void SetApiKey(string apiKey);
    public Task<bool> TestConnection();
}

public class HttpClientFactory
{
    private static readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(() => CreateHttpClient());

    public static HttpClient Instance => _httpClient.Value;

    private HttpClientFactory()
    {
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();

        // Set the timeout for requests 
        httpClient.Timeout = TimeSpan.FromSeconds(30);

        // Set default request headers (optional)
        //httpClient.DefaultRequestHeaders.Accept.Clear();
        //httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("multipart/form-data"));

        return httpClient;
    }
}