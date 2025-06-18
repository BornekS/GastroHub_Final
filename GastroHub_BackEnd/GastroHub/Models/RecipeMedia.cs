namespace GastroHub.Models
{
    public class RecipeMedia
        {
        public int Id { get; set; }
        public string Url { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}