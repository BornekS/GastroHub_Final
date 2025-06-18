using System;
using System.Collections.Generic;

namespace GastroHub.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Ingredients { get; set; }
        public string Instructions { get; set; }
        public int PreparationTimeMinutes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<RecipeLike> Likes { get; set; } = new List<RecipeLike>();
        public ICollection<RecipeMedia> Media { get; set; } = new List<RecipeMedia>();
    }
}