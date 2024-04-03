using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using VXMusic.AudD;
using System.Diagnostics;

namespace VXMusic;

public class AudDResponse : IRecognitionApiClientResponse
{
    public Status Status { get; set; }
    public Result? Result { get; set; }
}

public class AudDHttpClient : IHttpClient
{
    private readonly string ApiEndpoint = "https://api.audd.io";
    private string _audDApiKey;

    public AudDHttpClient()
    {
        string currentDirectory = Environment.CurrentDirectory;
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("C:\\Users\\Tam\\RiderProjects\\VXMusic\\VXMusic\\appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        _audDApiKey = config["AudD:ApiToken"];
    }

    public async Task<IRecognitionApiClientResponse> GetArtist(byte[] audioBytes)
    {
        using (var requestContent = new MultipartFormDataContent())
        {
            // Add the API token and 'return' fields to the JSON body
            var jsonBody = new
            {
                api_token = _audDApiKey,
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
            var response = await HttpClientFactory.Instance.PostAsync(ApiEndpoint + "/recognize", requestContent);

            if (response.IsSuccessStatusCode)
            {
                Trace.WriteLine("Success");
            }

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                // TODO Handle Exit codes!
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = await response.Content.ReadAsStringAsync();

                var parsedResult = JsonConvert.DeserializeObject<AudDResponse>(result);

                if (parsedResult.Status == Status.Error) // This is probably broken after refactorign the status to an enum
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

    public void SetApiKey(string apiKey)
    {
        _audDApiKey = apiKey;
    }

    public async Task<bool> TestConnection()
    {
        using (var requestContent = new MultipartFormDataContent())
        {
            // Add the API token and 'return' fields to the JSON body
            var jsonBody = new
            {
                api_token = _audDApiKey,
                q = "king of morter king of bricks, the river styx is a river of stones"
            };

            var jsonBodyString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonBody);
            requestContent.Add(new StringContent(jsonBodyString, Encoding.UTF8, "application/json"), "json");

            // Make the POST request to the API
            var response = await HttpClientFactory.Instance.PostAsync(ApiEndpoint + "/findLyrics", requestContent);

            response.EnsureSuccessStatusCode();

            return response.IsSuccessStatusCode;
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
                    api_token = _audDApiKey,
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
                    Trace.WriteLine("Success");
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