using System.Collections.Generic;

namespace GastroHub.Dtos.Ingredients
{
    public class CreateIngredientDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Category { get; set; }
        public int Unit { get; set; }
        public double QuantityPerUnit { get; set; }
        public int CaloriesPer100g { get; set; }
        public List<string> Allergens { get; set; } = new List<string>();
        public decimal? PricePerUnit { get; set; }

        public string? ImageUrl { get; set; }
    }
}