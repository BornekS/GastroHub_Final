using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GastroHub.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        [Required]
        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int? ParentCommentId { get; set; }
        public Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public ICollection<CommentLike> Likes { get; set; } = new List<CommentLike>();
    }
}
