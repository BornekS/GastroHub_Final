using System.Collections.Generic;
using System.Threading.Tasks;
using GastroHub.Dtos.Comments;

namespace GastroHub.Services.Interfaces
{
    public interface ICommentService
    {
        Task<List<CommentDto>> GetForRecipeAsync(int recipeId, string? userEmail);

        Task<CommentDto> AddAsync(int recipeId, CreateCommentDto dto, string userEmail);

        Task LikeAsync(int commentId, string userEmail);

        Task UnlikeAsync(int commentId, string userEmail);
    }
}
