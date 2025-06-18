using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using GastroHub.Dtos.Categories;
using GastroHub.Services.Interfaces;

namespace GastroHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _cats;

        public CategoriesController(ICategoryService cats) => _cats = cats;

        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAll()
        {
            var list = await _cats.GetAllAsync();
            return Ok(list);
        }
    }
}
