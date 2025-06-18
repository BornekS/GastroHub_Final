namespace GastroHub.Models
{
    public class RecipeLike
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }
}
