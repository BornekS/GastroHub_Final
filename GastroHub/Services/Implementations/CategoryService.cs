using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GastroHub.Data;
using GastroHub.Dtos.Categories;
using GastroHub.Models;
using GastroHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroHub.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public CategoryService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<CategoryDto>> GetAllAsync()
        {
            var cats = await _db.Categories.AsNoTracking().ToListAsync();
            return _mapper.Map<List<CategoryDto>>(cats);
        }
    }
}
