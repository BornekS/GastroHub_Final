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
    public class LikesController : ControllerBase
    {
        private readonly IRecipeService _recipes;
        private readonly ICommentService _comments;

        public LikesController(IRecipeService recipes, ICommentService comments)
        {
            _recipes = recipes;
            _comments = comments;
        }

        [HttpPost("recipe/{recipeId}")]
        public async Task<IActionResult> LikeRecipe(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _recipes.LikeAsync(recipeId, userId);
            return NoContent();
        }

        [HttpDelete("recipe/{recipeId}")]
        public async Task<IActionResult> UnlikeRecipe(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _recipes.UnlikeAsync(recipeId, userId);
            return NoContent();
        }

        [HttpPost("comment/{commentId}")]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _comments.LikeAsync(commentId, userId);
            return NoContent();
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> UnlikeComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _comments.UnlikeAsync(commentId, userId);
            return NoContent();
        }
    }
}
