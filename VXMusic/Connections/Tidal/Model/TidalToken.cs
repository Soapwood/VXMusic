using Newtonsoft.Json;

namespace VXMusic.Tidal.Model;

public class TidalToken
{
    [JsonProperty("access_token")]
    public string AccessToken;
    [JsonProperty("token_type")]
    public string TokenType;
    [JsonProperty("expires_in")]
    public int ExpiresIn;
}