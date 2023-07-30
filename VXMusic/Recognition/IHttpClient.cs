using Disco.Business.Interfaces.Dtos.AudD;
using Disco.Integration.Interfaces.Dtos.AudD;

namespace VXMusic;

public interface IHttpClient
{
    public Task<IApiClientResponse> GetArtist(byte[] audioBytes);
    
}

public interface IApiClientResponse
{
    public string status { get; set; }
    public Result result { get; set; }
}

public class Result
{
    public string artist { get; set; }
    public string title { get; set; }
    public string album { get; set; }
    public string release_date { get; set; }
    public string label { get; set; }
    public string timecode { get; set; }
    public string song_link { get; set; }
    //public AppleMusic apple_music { get; set; }
    //public Spotify spotify { get; set; }
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