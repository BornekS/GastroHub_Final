using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using GastroHub.Dtos.Comments;
using GastroHub.Services.Interfaces;

namespace GastroHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _comments;
        public CommentsController(ICommentService comments) => _comments = comments;

        [HttpGet("recipe/{recipeId}")]
        public async Task<ActionResult<List<CommentDto>>> GetByRecipe(int recipeId)
        {
            string? userEmail = User.Identity?.IsAuthenticated == true
                ? User.FindFirstValue("sub")
                : null;

            var list = await _comments.GetForRecipeAsync(recipeId, userEmail);
            return Ok(list);
        }

        [Authorize]
        [HttpPost("recipe/{recipeId}")]
        public async Task<ActionResult<CommentDto>> Create(
            int recipeId,
            [FromBody] CreateCommentDto dto)
        {

            string userEmail = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("sub claim missing!");

            var comment = await _comments.AddAsync(recipeId, dto, userEmail);
            return Ok(comment);
        }
    }
}
