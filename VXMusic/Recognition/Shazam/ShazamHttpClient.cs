using System.Net.Http.Headers;
using NAudio.Wasapi.CoreAudioApi.Interfaces;
using Newtonsoft.Json;

namespace VXMusic.Recognition.Shazam;

public class ShazamResponse : IApiClientResponse 
{
    public string status { get; set; }
    public Result result { get; set; }
}

public class ShazamHttpClient
{
    public async Task<IApiClientResponse> GetArtist(string audioDataBase64)
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

            ShazamHttpResponse shazamResponse = JsonConvert.DeserializeObject<ShazamHttpResponse>(body);

            // get spotify link
            string spotifyLink = "";
            foreach(var provider in shazamResponse.track.hub.providers)
            {
                if(provider.type == "SPOTIFY")
                {
                    foreach (var action in provider.actions)
                    {
                        if (action.type == "uri")
                            spotifyLink = action.uri;
                    }
                }
            } 
            
            return new ShazamResponse()
            {
                status = "success",
                result = new Result()
                {
                    artist = shazamResponse.track.subtitle,
                    title = shazamResponse.track.title,
                    album = "album",
                    release_date = "release_date",
                    label = "label",
                    timecode = "timecode",
                    song_link = spotifyLink
                }
            };
        }
    }
    
    
}