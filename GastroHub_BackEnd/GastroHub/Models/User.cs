using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace GastroHub.Models;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<RecipeLike> LikedRecipes { get; set; } = new List<RecipeLike>();

}
