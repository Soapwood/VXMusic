using com.csutil.http.apis;
using SpotifyAPI.Web;

namespace VXMusic.Spotify;

using Disco.Business.Interfaces.Dtos.AudD;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SpotifyPlaylistManager
{
    public enum SpotifyPlaylistReponse
    {
        TrackAdded,
        TrackFailedToAdd,
        TrackAlreadyExists
    }
    
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

    public static async Task<SpotifyPlaylistReponse> AddTrackToPlaylist(string playlistId, PlaylistAddItemsRequest playlistAddItemsRequest)
    {
        if (SpotifyClientBuilder.Instance == null)
            throw new Exception("Not authenticated with Spotify API."); // TODO Handle this, and output to UI

        var spotify = await SpotifyClientBuilder.Instance;

        if (await IsTrackAlreadyInPlaylist(spotify, playlistId, playlistAddItemsRequest))
            return SpotifyPlaylistReponse.TrackAlreadyExists;

        await spotify.Playlists.AddItems(playlistId, playlistAddItemsRequest);

        return SpotifyPlaylistReponse.TrackAdded;
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

    private static async Task<bool> IsTrackAlreadyInPlaylist(SpotifyClient instance, string playlistId,
        PlaylistAddItemsRequest trackToAdd)
    {
        var existingTracks = await instance.Playlists.GetItems(playlistId);

        return existingTracks.Items.Any(track => {
            FullTrack trackToCheck = (FullTrack) track.Track;
            // This is on the assumption that there will only be one track added at a time.
            return trackToCheck.Uri == trackToAdd.Uris.FirstOrDefault();
        });
    }
}
