using SpotifyAPI.Web;

namespace VXMusic.Spotify;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SpotifyPlaylistManager
{
    public async Task<string> CreatePlaylist(string userId, string playlistName, bool isPublic = true)
    {
        if (SpotifyClientBuilder.Instance == null)
            throw new Exception("Not authenticated with Spotify API.");

        //var newPlaylist = await SpotifyClientBuilder.Instance.Playlists.Create()CreatePlaylistAsync(userId, playlistName, publicPlaylist: isPublic);
        //return newPlaylist.Id;
        return null;
    }

    public async Task<bool> AddTrackToPlaylist(string playlistId, string trackUri)
    {
        if (SpotifyClientBuilder.Instance == null)
            throw new Exception("Not authenticated with Spotify API.");

        var uris = new List<string> { trackUri };
        //var snapshotId = await SpotifyClientBuilder.Instance.AddPlaylistTracksAsync(playlistId, uris);
        //return !string.IsNullOrEmpty(snapshotId);
        return false; 
    }
}
