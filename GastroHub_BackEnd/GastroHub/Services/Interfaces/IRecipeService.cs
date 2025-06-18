using System.Collections.Generic;
using System.Threading.Tasks;
using GastroHub.Dtos.Recipes;
using GastroHub.Helpers;

namespace GastroHub.Services.Interfaces
{
    public interface IRecipeService
    {
        Task<PagedList<RecipeDto>> GetPagedAsync(
            RecipeFilterParams filterParams,
            string? currentUserId);
        Task<RecipeDto> GetByIdAsync(int id, string? currentUserId);
        Task<RecipeDto> CreateAsync(CreateRecipeDto dto, string userId);
        Task UpdateAsync(int id, CreateRecipeDto dto, string userId);
        Task DeleteAsync(int id, string userId);
        Task LikeAsync(int recipeId, string userId);
        Task UnlikeAsync(int recipeId, string userId);
        Task<IReadOnlyList<RecipeDto>> GetByUserAsync(
            string authorId,
            string? currentUserId);
    }
}
