namespace VXMusic.Connections.Http;

public class VxHttpClient
{
    private static readonly HttpClient _client = new HttpClient();

    private static readonly string VxNewsGistUrl =
        "https://gist.githubusercontent.com/Soapwood/0138ce4ac2370c435582c5574679bd3c/raw"; 
    
    public static async Task<string> FetchVxNewsGistContent()
    {
        try
        {
            // Replace the URL with your actual gist URL
            string gistUrl = VxNewsGistUrl;
            string responseBody = await _client.GetStringAsync(gistUrl);

            // Inject the fetched text into the TextBlock
            return responseBody;
        }
        catch (HttpRequestException e)
        {
            return "Could not fetch VXMusic News.";
        }
    }
}