using System.ComponentModel.DataAnnotations;

namespace GastroHub.Dtos.Recipes
{
    public class CreateRecipeDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public string Ingredients { get; set; }

        [Required]
        public string Instructions { get; set; }

        [Range(0, 1000)]
        public int PreparationTimeMinutes { get; set; }

        public int CategoryId { get; set; }

        public string ImageUrl { get; set; }
    }
}