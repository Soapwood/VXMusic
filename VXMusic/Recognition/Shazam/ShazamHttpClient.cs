using System.Net;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Swan.Logging;

namespace VXMusic.Recognition.Shazam;

public class ShazamResponse : IRecognitionApiClientResponse 
{
    public Status Status { get; set; }
    public Result Result { get; set; }
}

public class ShazamHttpClient
{
    private HttpClient _shazamHttpClient;
    private string _shazamApiKey;

    public ShazamHttpClient(string apiKey)
    {
        _shazamHttpClient = new HttpClient();
        _shazamApiKey = apiKey;
    }

    public void SetApiKey(string apiKey)
    {
        _shazamApiKey = apiKey;
    }
    
    public async Task<IRecognitionApiClientResponse> GetArtist(string audioDataBase64)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://shazam.p.rapidapi.com/songs/detect"), // TODO Add origin. Does this latency of response?
            Headers =
            {
                { "X-RapidAPI-Key", _shazamApiKey },
                { "X-RapidAPI-Host", "shazam.p.rapidapi.com" },
            },
            Content = new StringContent(audioDataBase64)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("text/plain")
                }
            }
        };
        
        using (var response = await _shazamHttpClient.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            // Log response message!
            ShazamHttpResponse shazamHttpResponse = JsonConvert.DeserializeObject<ShazamHttpResponse>(body);

            if(shazamHttpResponse.matches.Count == 0)
                return new ShazamResponse() { Status = Status.NoMatches };

            return ShazamUtils.ParseShazamResponse(shazamHttpResponse);
        }
    }
    
    public async Task<bool> TestApiServiceConnection()
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://shazam.p.rapidapi.com/charts/list"), // TODO Add origin. Does this latency of response?
            Headers =
            {
                { "X-RapidAPI-Key", _shazamApiKey },
                { "X-RapidAPI-Host", "shazam.p.rapidapi.com" },
            },
        };
        
        using (var response = await _shazamHttpClient.SendAsync(request))
        {
            // TODO Add deserialiser for this request
            if (!response.EnsureSuccessStatusCode().IsSuccessStatusCode)
                return false;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }
    }
    
}