using Chinook.Models;
using Chinook.ClientModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Chinook.Shared.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Chinook.Pages
{
    public partial class ArtistPage
    {
        [Parameter] public long ArtistId { get; set; }
        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }
        private Modal PlaylistDialog { get; set; }

        private Artist Artist;
        private List<PlaylistTrack> Tracks;
        private DbContext DbContext;
        private PlaylistTrack SelectedTrack;
        private string InfoMessage;
        private string CurrentUserId;
        public async Task OnInitializedAsync()
        {
            CurrentUserId = await GetUserId();
            var DbContext = await DbFactory.CreateDbContextAsync();

            Artist = DbContext.Artists.SingleOrDefault(a => a.ArtistId == ArtistId);

            Tracks = DbContext.Tracks.Where(a => a.Album.ArtistId == ArtistId)
                .Include(a => a.Album)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == CurrentUserId && up.Playlist.Name == "Favorites")).Any()
                })
            .ToList();
        }
        public async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        public void FavoriteTrack(long trackId)
        {
            var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} added to playlist Favorites.";
        }

        public void UnfavoriteTrack(long trackId)
        {
            var track = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
        }

        public void OpenPlaylistDialog(long trackId)
        {
            CloseInfoMessage();
            SelectedTrack = Tracks.FirstOrDefault(t => t.TrackId == trackId);
            PlaylistDialog.Open();
        }

        public void AddTrackToPlaylist()
        {
            CloseInfoMessage();
            InfoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to playlist {{playlist name}}.";
            PlaylistDialog.Close();
        }

        public void CloseInfoMessage()
        {
            InfoMessage = "";
        }
    }
}
