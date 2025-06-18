namespace GastroHub.Models
{
    public class Favorite
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
