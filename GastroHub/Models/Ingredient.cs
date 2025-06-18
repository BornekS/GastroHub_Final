// Models/Ingredient.cs
using System;
using System.Collections.Generic;

namespace GastroHub.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IngredientCategory Category { get; set; }
        public UnitType Unit { get; set; }
        public double QuantityPerUnit { get; set; }
        public int CaloriesPer100g { get; set; }
        public double ProteinPer100g { get; set; }
        public double FatPer100g { get; set; }
        public double CarbohydratesPer100g { get; set; }
        public List<string> Allergens { get; set; } = new List<string>();
        public decimal? PricePerUnit { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public string? ImageUrl { get; set; }
    }

    public enum IngredientCategory
    {
        Dairy,
        Fruit,
        Vegetable,
        Meat,
        Grain,
        Spice,
        Other
    }

    public enum UnitType
    {
        Gram,
        Kilogram,
        Milliliter,
        Liter,
        Piece,
        Teaspoon,
        Tablespoon,
        Cup
    }
}