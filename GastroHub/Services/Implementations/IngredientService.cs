// Services/Implementations/IngredientService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GastroHub.Data;
using GastroHub.Dtos.Ingredients;
using GastroHub.Models;
using GastroHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GastroHub.Services.Implementations
{
    public class IngredientService : IIngredientService
    {
        private readonly ApplicationDbContext _db;

        public IngredientService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<IngredientDto>> GetAllAsync()
        {
            return await _db.Ingredients
                .AsNoTracking()
                .Select(i => new IngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Category = i.Category.ToString(),
                    Unit = i.Unit.ToString(),
                    QuantityPerUnit = i.QuantityPerUnit,
                    CaloriesPer100g = i.CaloriesPer100g,
                    Allergens = i.Allergens,
                    PricePerUnit = i.PricePerUnit,
                    ImageUrl = i.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<IngredientDto?> GetByIdAsync(int id)
        {
            var i = await _db.Ingredients.FindAsync(id);
            if (i == null) return null;

            return new IngredientDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Category = i.Category.ToString(),
                Unit = i.Unit.ToString(),
                QuantityPerUnit = i.QuantityPerUnit,
                CaloriesPer100g = i.CaloriesPer100g,
                Allergens = i.Allergens,
                PricePerUnit = i.PricePerUnit,
                ImageUrl = i.ImageUrl
            };
        }

        public async Task<IngredientDto> CreateAsync(CreateIngredientDto dto)
        {
            var ingredient = new Ingredient
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = (IngredientCategory)dto.Category,
                Unit = (UnitType)dto.Unit,
                QuantityPerUnit = dto.QuantityPerUnit,
                CaloriesPer100g = dto.CaloriesPer100g,
                Allergens = dto.Allergens,
                PricePerUnit = dto.PricePerUnit,
                ImageUrl = dto.ImageUrl
            };

            _db.Ingredients.Add(ingredient);
            await _db.SaveChangesAsync();

            return new IngredientDto
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Description = ingredient.Description,
                Category = ingredient.Category.ToString(),
                Unit = ingredient.Unit.ToString(),
                QuantityPerUnit = ingredient.QuantityPerUnit,
                CaloriesPer100g = ingredient.CaloriesPer100g,
                Allergens = ingredient.Allergens,
                PricePerUnit = ingredient.PricePerUnit,
                ImageUrl = ingredient.ImageUrl
            };
        }

        public async Task UpdateAsync(int id, CreateIngredientDto dto)
        {
            var ingredient = await _db.Ingredients.FindAsync(id);
            if (ingredient == null) return;

            ingredient.Name = dto.Name;
            ingredient.Description = dto.Description;
            ingredient.Category = (IngredientCategory)dto.Category;
            ingredient.Unit = (UnitType)dto.Unit;
            ingredient.QuantityPerUnit = dto.QuantityPerUnit;
            ingredient.CaloriesPer100g = dto.CaloriesPer100g;
            ingredient.Allergens = dto.Allergens;
            ingredient.PricePerUnit = dto.PricePerUnit;
            ingredient.ImageUrl = dto.ImageUrl;
            ingredient.UpdatedAt = DateTime.UtcNow;

            _db.Ingredients.Update(ingredient);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var ingredient = await _db.Ingredients.FindAsync(id);
            if (ingredient == null) return;

            _db.Ingredients.Remove(ingredient);
            await _db.SaveChangesAsync();
        }
    }
}
