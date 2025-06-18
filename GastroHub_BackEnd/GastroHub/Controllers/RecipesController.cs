using GastroHub.Dtos.Recipes;
using GastroHub.Helpers;
using GastroHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GastroHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipes;

        public RecipesController(IRecipeService recipes) => _recipes = recipes;

        [HttpGet]
        public async Task<ActionResult<PagedList<RecipeDto>>> GetAll([FromQuery] RecipeFilterParams fp)
        {
            string? currentUserEmail = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                : null;

            var list = await _recipes.GetPagedAsync(fp, currentUserEmail);
            return Ok(list);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<RecipeDto>> Get(int id)
        {
            string? currentUserEmail = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                : null;

            var recipe = await _recipes.GetByIdAsync(id, currentUserEmail);
            return recipe is null ? NotFound("Recipe not found.") : Ok(recipe);
        }

        [HttpGet("user/{authorId}")]
        public async Task<ActionResult<IEnumerable<RecipeDto>>> GetByUser(string authorId)
        {
            string? currentUserEmail = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                : null;

            var list = await _recipes.GetByUserAsync(authorId, currentUserEmail);
            return Ok(list);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RecipeDto>> Create(CreateRecipeDto dto)
        {
            var userEmail = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("User not authenticated.");

            var created = await _recipes.CreateAsync(dto, userEmail);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CreateRecipeDto dto)
        {
            var userEmail = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("User not authenticated.");

            await _recipes.UpdateAsync(id, dto, userEmail);
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userEmail = User.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            if (string.IsNullOrEmpty(userEmail))
                return Unauthorized("User not authenticated.");

            await _recipes.DeleteAsync(id, userEmail);
            return NoContent();
        }
    }
}
