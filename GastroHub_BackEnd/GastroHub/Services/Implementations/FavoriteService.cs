using System.Threading.Tasks;
using GastroHub.Data;
using GastroHub.Models;
using GastroHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroHub.Services.Implementations
{
    public class FavoriteService : IFavoriteService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(ApplicationDbContext db,
                               ILogger<FavoriteService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<bool> AddAsync(int recipeId, string userId)
        {
            if (!await _db.Recipes.AnyAsync(r => r.Id == recipeId))
                return false;       

            if (!await _db.Users.AnyAsync(u => u.Id == userId))
                return false;          

            if (await _db.Favorites.AnyAsync(f => f.RecipeId == recipeId && f.UserId == userId))
                return false;         

            _db.Favorites.Add(new Favorite { RecipeId = recipeId, UserId = userId });
            await _db.SaveChangesAsync();
            return true;              
        }

        public async Task<bool> RemoveAsync(int recipeId, string userId)
        {
            var fav = await _db.Favorites
                               .FirstOrDefaultAsync(f => f.RecipeId == recipeId && f.UserId == userId);
            if (fav is null) return false;        // nothing to delete

            _db.Favorites.Remove(fav);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
