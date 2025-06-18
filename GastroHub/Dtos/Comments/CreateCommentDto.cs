using System.ComponentModel.DataAnnotations;

namespace GastroHub.Dtos.Comments
{
    public class CreateCommentDto
    {
        [Required]
        public string Content { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
