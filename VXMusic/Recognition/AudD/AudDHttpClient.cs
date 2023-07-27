using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using VXMusic.AudD;

namespace VXMusic;

public class AudDResponse : IApiClientResponse
{
    public string? status { get; set; }
    public Result? result { get; set; }
}

public class AudDHttpClient : IHttpClient
{
    private readonly string ApiEndpoint = "https://api.audd.io/recognize";
    private readonly string ApiToken;

    public AudDHttpClient()
    {
        string currentDirectory = Environment.CurrentDirectory;
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("C:\\Users\\Tam\\RiderProjects\\VXMusic\\VXMusic\\appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        ApiToken = config["AudD:ApiToken"];
    }

    public async Task<IApiClientResponse> GetArtistFromByteArray(byte[] audioBytes)
    {
        using (var requestContent = new MultipartFormDataContent())
        {
            // Add the API token and 'return' fields to the JSON body
            var jsonBody = new
            {
                api_token = ApiToken,
                @return = "spotify"
            };

            var jsonBodyString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonBody);
            requestContent.Add(new StringContent(jsonBodyString, Encoding.UTF8, "application/json"), "json");

            // Add the audio file as a separate file parameter
            var fileContent = new ByteArrayContent(audioBytes);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue("audio/mpeg"); // Change this content type if required
            requestContent.Add(fileContent, "file", "randomaudio");

            // Make the POST request to the API
            var response = await HttpClientFactory.Instance.PostAsync(ApiEndpoint, requestContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Success");
            }

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                // TODO Handle Exit codes!
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = await response.Content.ReadAsStringAsync();

                var parsedResult = JsonConvert.DeserializeObject<AudDResponse>(result);

                if (parsedResult.status == "error")
                {
                    AudDErrorResponseHandler.HandleAudDErrorResponse(responseContent);
                }

                return parsedResult;
            }
            else
            {
                return null;
            }
        }
    }

    public async Task<AudDResponse> GetArtist(string audioFilePath)
    {
        FileInfo audioFile = new FileInfo(audioFilePath);

        using (var httpClient = new HttpClient())
        {
            using (var requestContent = new MultipartFormDataContent())
            {
                // Add the API token and 'return' fields to the JSON body
                var jsonBody = new
                {
                    api_token = ApiToken,
                    @return = "spotify"
                };

                var jsonBodyString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonBody);
                requestContent.Add(new StringContent(jsonBodyString, Encoding.UTF8, "application/json"), "json");

                // Add the audio file as a separate file parameter
                var fileContent = new StreamContent(audioFile.OpenRead());
                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue("audio/mpeg"); // Change this content type if required
                requestContent.Add(fileContent, "file", "randomaudio");

                // Make the POST request to the API
                var response = await httpClient.PostAsync(ApiEndpoint, requestContent);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Success");
                }

                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AudDResponse>(result);
                    ;
                }
                else
                {
                    // Handle the error here if needed
                    return null;
                }
            }
        }
    }
}