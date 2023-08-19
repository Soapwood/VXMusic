using System.Diagnostics;
using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using SpotifyAPI.Web.Auth;
using VXMusic.Lastfm;

namespace VXMusic.Lastfm.Authentication;

public class LastfmAuthentication
{
    public static string? ClientId { get; set; } 
    public static string? ClientSecret { get; set; }
}