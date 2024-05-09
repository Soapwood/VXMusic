using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VXMusic.Recognition.Shazam;

public class ShazamUtils
{
    public static async Task<ShazamResponse> ParseShazamResponse(ShazamHttpResponse shazamHttpResponse)
    {
        ShazamResponse shazamResponse = new ShazamResponse()
        {
            Result = new Result()
        };

        shazamResponse.Result.Artist = shazamHttpResponse.track.subtitle;
        shazamResponse.Result.Title = shazamHttpResponse.track.title;

        if(!string.IsNullOrEmpty(shazamHttpResponse.track.images.coverart))
            shazamResponse.Result.AlbumArt = await ConvertImgUrlToBase64(shazamHttpResponse.track.images.coverart);

        foreach (var section in shazamHttpResponse.track.sections)
        {
            if (section.ContainsKey("type") && section.ContainsValue("SONG"))
            {
                foreach (JObject item in JArray.Parse(section["metadata"].ToString()))
                {
                    if(item.GetValue("title").ToString() == "Album")
                        shazamResponse.Result.Album = item.GetValue("text").ToString();
                    if(item.GetValue("title").ToString() == "Released")
                        shazamResponse.Result.ReleaseDate = item.GetValue("text").ToString();
                    if(item.GetValue("title").ToString() == "Label")
                        shazamResponse.Result.Label = item.GetValue("text").ToString();
                }
            }

            if (shazamResponse.Result.Album != null && shazamResponse.Result.ReleaseDate != null &&
                shazamResponse.Result.Label != null)
                break;
        }
        
        // get spotify link
        string spotifyLink = "";
        foreach(var provider in shazamHttpResponse.track.hub.providers)
        {
            if(provider.type == "SPOTIFY")
            {
                foreach (var action in provider.actions)
                {
                    if (action.type == "uri")
                        shazamResponse.Result.SongLink = action.uri;
                }
            }

            if (shazamResponse.Result.SongLink != null)
                break;
        }

        return shazamResponse;
    }
    
    private static async Task<string> ConvertImgUrlToBase64(string imgUrl)
    {
        using (HttpClient client = new HttpClient())
        {
            // Get the image data from the URL
            byte[] imageData = await client.GetByteArrayAsync(imgUrl);

            // Convert the data to an Image object
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                using (Image image = Image.FromStream(ms))
                {
                    // Convert Image to Base64
                    using (MemoryStream msOutput = new MemoryStream())
                    {
                        image.Save(msOutput, ImageFormat.Jpeg);
                        byte[] imageBytes = msOutput.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);
                        return base64String;
                    }
                }
            }
        }
    }
}