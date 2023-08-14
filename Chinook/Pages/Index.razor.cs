using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Chinook.Pages
{
    public partial class Index
    {
        private List<Artist> Artists;
        [Inject] IDbContextFactory<ChinookContext> DbFactory { get; set; }

        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
            Artists = await GetArtists();
        }

        public async Task<List<Artist>> GetArtists()
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            var users = dbContext.Users.Include(a => a.UserPlaylists).ToList();

            return dbContext.Artists.ToList();
        }

        public async Task GetArtistByName(string artistName)
        {
            var CurrentUserId = await GetUserId();
            var dbContext = await DbFactory.CreateDbContextAsync();

            Artists = dbContext.Artists.Where(a => a.Name == artistName)
            .ToList();
        }
        public async Task<List<Album>> GetAlbumsForArtist(int artistId)
        {
            var dbContext = await DbFactory.CreateDbContextAsync();
            return dbContext.Albums.Where(a => a.ArtistId == artistId).ToList();
        }

        public async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }
    }
}
