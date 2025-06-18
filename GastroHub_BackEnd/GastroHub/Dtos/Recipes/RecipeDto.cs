public class RecipeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Ingredients { get; set; }
    public string Instructions { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public string CategoryName { get; set; }

    public string UserId { get; set; } = default!;
    public string UserDisplayName { get; set; }
    public int LikesCount { get; set; }
    public bool IsFavorite { get; set; }  // Whether the current user has favorited the recipe
    public bool LikedByCurrentUser { get; set; }  // Whether the current user has liked the recipe
    public List<string> ImageUrls { get; set; } // If you want to include image URLs
}