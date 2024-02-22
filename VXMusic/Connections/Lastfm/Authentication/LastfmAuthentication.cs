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

    public static string EncryptionPassphrase = "Bongodrumpwankum";

    public static string LastFmCredentialsPath = "credentials.dat";
    public static bool CredentialFileExists => File.Exists(LastFmCredentialsPath);


    public static async Task<bool> Login(string username, string password)
    {
        var lastfm = await LastfmClientBuilder.Instance;

        if (CredentialFileExists)
        {
            var credentials = CredentialsEncryption.RetrieveCredentials(EncryptionPassphrase);
        }
        else
        {
            CredentialsEncryption.StoreCredentials(username, password, EncryptionPassphrase);
        }

        var lastResponse = await lastfm.Auth.GetSessionTokenAsync(username, password);
        return lastfm.Auth.Authenticated;
    }
}