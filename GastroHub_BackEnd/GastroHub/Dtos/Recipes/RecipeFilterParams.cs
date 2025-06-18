using System.Collections.Generic;

namespace GastroHub.Dtos.Recipes
{
    public class RecipeFilterParams
    {
        public string? Query { get; set; }
        public List<string>? IncludeIngredients { get; set; }
        public List<string>? ExcludeIngredients { get; set; }
        public int? MinPrepTime { get; set; }
        public int? MaxPrepTime { get; set; }
        public int? CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}