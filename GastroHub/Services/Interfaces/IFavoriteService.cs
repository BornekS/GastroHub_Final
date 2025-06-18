using System.Threading.Tasks;

namespace GastroHub.Services.Interfaces
{
    public interface IFavoriteService
    {
        Task<bool> AddAsync(int recipeId, string userId);
        Task<bool> RemoveAsync(int recipeId, string userId);
    }
}
