using Newtonsoft.Json;

namespace VXMusic.Spotify.Model;

public class SpotifyToken
{
    [JsonProperty("access_token")]
    public string AccessToken;
    [JsonProperty("token_type")]
    public string TokenType;
    [JsonProperty("expires_in")]
    public int ExpiresIn;
}