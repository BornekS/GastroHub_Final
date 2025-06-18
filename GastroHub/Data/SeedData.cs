using GastroHub.Models;
using GastroHub.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GastroHub.Data;

public static class SeedData
{
    public static async Task SeedCategories(ApplicationDbContext context)
    {
        if (!context.Categories.Any())
        {
            var categories = new[]
            {
                "Deserti",
                "Glavna jela",
                "Kruh i peciva",
                "Topla predjela",
                "Prilozi i variva",
                "Hladna predjela",
                "Salate",
                "Juhe",
                "Pića",
                "Zimnica",
                "Umaci, dipovi i salatni preljevi",
                "Brza jela",
                "Tjestenine",
                "Rižota",
                "Roštilj",
                "Vegansko",
                "Bezglutensko",
                "Riba i plodovi mora",
                "Smoothieji i shakeovi",
                "Dječji meni"
            };

            context.Categories.AddRange(
                categories.Select(name => new Category { Name = name })
            );

            await context.SaveChangesAsync();
        }
    }
}
