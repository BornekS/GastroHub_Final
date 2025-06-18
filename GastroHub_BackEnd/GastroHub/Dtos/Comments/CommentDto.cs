using System;
using System.Collections.Generic;

namespace GastroHub.Dtos.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string UserDisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RecipeId { get; set; }
        public int? ParentCommentId { get; set; }
        public int LikesCount { get; set; }
        public bool LikedByCurrentUser { get; set; }
        public List<CommentDto> Replies { get; set; } = new();
    }
}
