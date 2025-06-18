using System.Collections.Generic;
using System.Threading.Tasks;
using GastroHub.Dtos.Categories;

namespace GastroHub.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryDto>> GetAllAsync();
    }
}