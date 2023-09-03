using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace VXMusic.Recognition.Shazam;

public class ShazamResponse : IRecognitionApiClientResponse 
{
    public Status Status { get; set; }
    public Result Result { get; set; }
}

public class ShazamHttpClient
{
    public async Task<IRecognitionApiClientResponse> GetArtist(string audioDataBase64)
    {
        var client = new HttpClient();

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri("https://shazam.p.rapidapi.com/songs/detect"), // TODO Add origin. Does this latency of response?
            Headers =
            {
                { "X-RapidAPI-Key", "bb058b1a1cmsh9026752692f380bp1ffca1jsnc94e79c37484" },
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
        using (var response = await client.SendAsync(request))
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
    
    
}