using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using GastroHub.Services.Interfaces;

namespace GastroHub.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService _favorites;

        public FavoritesController(IFavoriteService favorites) => _favorites = favorites;

        [HttpPost("{recipeId}")]
        public async Task<IActionResult> Add(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            bool created = await _favorites.AddAsync(recipeId, userId);

            return created
                ? StatusCode(201)      
                : NoContent();
        }


        [HttpDelete("{recipeId}")]
        public async Task<IActionResult> Remove(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            bool removed = await _favorites.RemoveAsync(recipeId, userId);

            return removed
                ? NoContent()
                : NotFound();
        }
    }
}
