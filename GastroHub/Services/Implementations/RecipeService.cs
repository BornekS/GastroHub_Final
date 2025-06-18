using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GastroHub.Data;
using GastroHub.Dtos.Recipes;
using GastroHub.Helpers;
using GastroHub.Models;
using GastroHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroHub.Services.Implementations
{
    public class RecipeService : IRecipeService
    {
        private readonly ApplicationDbContext _db;

        public RecipeService(ApplicationDbContext db) => _db = db;
        public async Task<RecipeDto> CreateAsync(CreateRecipeDto dto, string userEmail)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail)
                       ?? throw new ArgumentException("User not found.");

            var recipe = new Recipe
            {
                Name = dto.Name,
                Ingredients = dto.Ingredients,
                Instructions = dto.Instructions,
                PreparationTimeMinutes = dto.PreparationTimeMinutes,
                CategoryId = dto.CategoryId,
                UserId = user.Id
            };

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                recipe.Media.Add(new RecipeMedia { Url = dto.ImageUrl, Recipe = recipe });

            _db.Recipes.Add(recipe);
            await _db.SaveChangesAsync();

            return MapToDto(recipe, user.Id);
        }

        public async Task<PagedList<RecipeDto>> GetPagedAsync(
            RecipeFilterParams fp,
            string? currentUserEmail)
        {
            var q = _db.Recipes
                .AsNoTracking()
                .IncludeAll()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(fp.Query))
            {
                var term = fp.Query.Trim().ToLower();
                q = q.Where(r => r.Name.ToLower().Contains(term));
            }

            if (fp.MinPrepTime.HasValue)
                q = q.Where(r => r.PreparationTimeMinutes >= fp.MinPrepTime);

            if (fp.MaxPrepTime.HasValue)
                q = q.Where(r => r.PreparationTimeMinutes <= fp.MaxPrepTime);

            if (fp.CategoryId.HasValue)
                q = q.Where(r => r.CategoryId == fp.CategoryId);

            q = q.OrderByDescending(r => r.Id);

            var page = await PagedList<Recipe>.CreateAsync(q, fp.PageNumber, fp.PageSize);

            static HashSet<string> CsvToSet(string? csv) =>
                csv?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .ToHashSet() ?? new();

            IEnumerable<Recipe> filtered = page;

            if (fp.IncludeIngredients?.Any() == true)
            {
                var wanted = fp.IncludeIngredients.SelectMany(CsvToSet).ToHashSet();
                filtered = filtered.Where(r => wanted.All(CsvToSet(r.Ingredients).Contains));
            }

            if (fp.ExcludeIngredients?.Any() == true)
            {
                var banned = fp.ExcludeIngredients.SelectMany(CsvToSet).ToHashSet();
                filtered = filtered.Where(r => !CsvToSet(r.Ingredients).Overlaps(banned));
            }

            var currentUserId = await ResolveUserIdAsync(currentUserEmail);
            var dtos = filtered.Select(r => MapToDto(r, currentUserId)).ToList();

            return new PagedList<RecipeDto>(dtos, page.TotalCount, fp.PageNumber, fp.PageSize);
        }

        public async Task<RecipeDto?> GetByIdAsync(int id, string? currentUserEmail)
        {
            var recipe = await _db.Recipes
                .AsNoTracking()
                .IncludeAll()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null) return null;

            var currentUserId = await ResolveUserIdAsync(currentUserEmail);
            return MapToDto(recipe, currentUserId);
        }

        public async Task UpdateAsync(int id, CreateRecipeDto dto, string userEmail)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail)
                       ?? throw new ArgumentException("User not found.");

            var recipe = await _db.Recipes
                                  .Include(r => r.Media)
                                  .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id)
                       ?? throw new InvalidOperationException("Recipe not found or unauthorized.");

            recipe.Name = dto.Name;
            recipe.Ingredients = dto.Ingredients;
            recipe.Instructions = dto.Instructions;
            recipe.PreparationTimeMinutes = dto.PreparationTimeMinutes;
            recipe.CategoryId = dto.CategoryId;

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var currentUrl = recipe.Media.FirstOrDefault()?.Url;

                if (!string.Equals(currentUrl, dto.ImageUrl, StringComparison.OrdinalIgnoreCase))
                {
                    _db.RemoveRange(recipe.Media);
                    recipe.Media.Clear();

                    recipe.Media.Add(new RecipeMedia { Url = dto.ImageUrl, Recipe = recipe });
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, string userEmail)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail)
                       ?? throw new ArgumentException("User not found.");

            var recipe = await _db.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id)
                         ?? throw new InvalidOperationException("Recipe not found or unauthorized.");

            _db.Recipes.Remove(recipe);
            await _db.SaveChangesAsync();
        }
        public async Task LikeAsync(int recipeId, string userEmail)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail)
                       ?? throw new ArgumentException("User not found.");

            var recipe = await _db.Recipes.Include(r => r.Likes)
                                          .FirstOrDefaultAsync(r => r.Id == recipeId)
                         ?? throw new InvalidOperationException("Recipe not found.");

            if (!recipe.Likes.Any(l => l.UserId == user.Id))
            {
                recipe.Likes.Add(new RecipeLike { RecipeId = recipeId, UserId = user.Id });
                await _db.SaveChangesAsync();
            }
        }

        public async Task UnlikeAsync(int recipeId, string userEmail)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == userEmail)
                       ?? throw new ArgumentException("User not found.");

            var recipe = await _db.Recipes.Include(r => r.Likes)
                                          .FirstOrDefaultAsync(r => r.Id == recipeId)
                         ?? throw new InvalidOperationException("Recipe not found.");

            var like = recipe.Likes.FirstOrDefault(l => l.UserId == user.Id);
            if (like != null)
            {
                recipe.Likes.Remove(like);
                await _db.SaveChangesAsync();
            }
        }
        public async Task<IReadOnlyList<RecipeDto>> GetByUserAsync(
            string authorId,
            string? currentUserEmail)
        {
            var currentUserId = await ResolveUserIdAsync(currentUserEmail);

            var items = await _db.Recipes
                .AsNoTracking()
                .IncludeAll()
                .Where(r => r.UserId == authorId)
                .OrderByDescending(r => r.Id)
                .ToListAsync();

            return items.Select(r => MapToDto(r, currentUserId)).ToList();
        }

        private RecipeDto MapToDto(Recipe r, string? currentUserId) => new()
        {
            Id = r.Id,
            Name = r.Name,
            Ingredients = r.Ingredients,
            Instructions = r.Instructions,
            PreparationTimeMinutes = r.PreparationTimeMinutes,

            CategoryName = r.Category?.Name,
            UserDisplayName = r.User?.DisplayName,
            UserId = r.UserId,

            LikesCount = r.Likes.Count,
            LikedByCurrentUser = currentUserId != null &&
                          r.Likes.Any(l => l.UserId == currentUserId),
            IsFavorite = currentUserId != null &&
                          r.Favorites.Any(f => f.UserId == currentUserId),

            ImageUrls = r.Media.Select(m => m.Url).ToList()
        };

        private async Task<string?> ResolveUserIdAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;

            return await _db.Users
                .Where(u => u.Email == email)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }
    }
    internal static class RecipeQueryExtensions
    {
        public static IQueryable<Recipe> IncludeAll(this IQueryable<Recipe> q) =>
            q.Include(r => r.Category)
             .Include(r => r.User)
             .Include(r => r.Media)
             .Include(r => r.Likes)
             .Include(r => r.Favorites);
    }
}