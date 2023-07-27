using System.Net.Http.Headers;

namespace VXMusic.Recognition.Shazam;

public class ShazamHttpClient : IHttpClient
{
    public async Task<IApiClientResponse> GetArtistFromByteArray(byte[] audioBytes)
    {
        var client = new HttpClient();

        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            RequestUri =
                new Uri("https://shazam.p.rapidapi.com/songs/v2/detect?timezone=America%2FChicago&locale=en-US"),
            Headers =
            {
                { "X-RapidAPI-Key", "SIGN-UP-FOR-KEY" },
                { "X-RapidAPI-Host", "shazam.p.rapidapi.com" },
            },
            Content = new StringContent(
                "\"Generate one on your own for testing and send the body with the content-type as text/plain\"")
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
            Console.WriteLine(body);
        }

        return null;
    }
}