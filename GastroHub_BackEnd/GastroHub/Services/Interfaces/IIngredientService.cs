using System.Collections.Generic;
using System.Threading.Tasks;
using GastroHub.Dtos.Ingredients;

namespace GastroHub.Services.Interfaces
{
    public interface IIngredientService
    {
        Task<IEnumerable<IngredientDto>> GetAllAsync();
        Task<IngredientDto> GetByIdAsync(int id);
        Task<IngredientDto> CreateAsync(CreateIngredientDto dto);
        Task UpdateAsync(int id, CreateIngredientDto dto);
        Task DeleteAsync(int id);
    }
}