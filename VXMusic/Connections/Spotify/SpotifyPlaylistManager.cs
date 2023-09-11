using SpotifyAPI.Web;

namespace VXMusic.Spotify;

using Disco.Business.Interfaces.Dtos.AudD;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SpotifyPlaylistManager
{
    public static async Task<FullPlaylist> CreatePlaylist(string userId, string playlistName, bool isPublic = true)
    {
        if (SpotifyClientBuilder.Instance == null)
            throw new Exception("Not authenticated with Spotify API.");  // TODO Handle this, and output to UI

        var spotify = await SpotifyClientBuilder.Instance;

        var newPlaylist = new PlaylistCreateRequest(playlistName);

        // TODO Make this injectable
        newPlaylist.Description = "Playlist created by VXMusic.";
        newPlaylist.Collaborative = false;
        newPlaylist.Public = false;

        FullPlaylist fullPlaylist = await spotify.Playlists.Create(userId, newPlaylist);

        return fullPlaylist;
    }

    public static async Task<bool> AddTrackToPlaylist(string playlistId, PlaylistAddItemsRequest playlistAddItemsRequest)
    {
        if (SpotifyClientBuilder.Instance == null)
            throw new Exception("Not authenticated with Spotify API."); // TODO Handle this, and output to UI

        var spotify = await SpotifyClientBuilder.Instance;

        var itemAddResponse = await spotify.Playlists.AddItems(playlistId, playlistAddItemsRequest);
        return itemAddResponse != null;
    }

    public static string GetPlaylistIdByNameIfExists(string playlistName, IList<SimplePlaylist> playlists)
    {
        if (SpotifyClientBuilder.Instance == null)
            throw new Exception("Not authenticated with Spotify API."); // TODO Handle this, and output to UI

        foreach(var playlist in playlists)
        {
            if(playlist.Name == playlistName)
            {
                return playlist.Id.ToString();
            }
        }

        return null;
    }
}
