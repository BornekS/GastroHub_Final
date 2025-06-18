namespace GastroHub.Models
{
    public class CommentLike
    {
        public int CommentId { get; set; }
        public Comment Comment { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
