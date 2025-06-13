using Microsoft.EntityFrameworkCore;
using System.Linq;
using GastroHub.Models;
using GastroHub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MailKit.Search;

namespace GestroHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        // Prikazivanje svih recepata s mogućnošću pretrage i sortiranja
        public async Task<IActionResult> Index(string sortOrder,string searchTerm)
        {
            // Dohvati sve recepte
            var recipesQuery = _context.Recipes.AsQueryable();

            // Pretraga
            if (!string.IsNullOrEmpty(searchTerm))
            {
                recipesQuery = recipesQuery.Where(r => r.Name.Contains(searchTerm) || r.Ingredients.Contains(searchTerm));
            }

            // Sortiranje prema vremenu slaganja
            switch (sortOrder)
            {
                case "time_asc":
                    recipesQuery = recipesQuery.OrderBy(r => r.PreparationTime);
                    break;
                case "time_desc":
                    recipesQuery = recipesQuery.OrderByDescending(r => r.PreparationTime);
                    break;
                default:
                    recipesQuery = recipesQuery.OrderBy(r => r.Name); // Podrazumijevano sortiranje prema imenu
                    break;
            }

            var recipes = await recipesQuery.ToListAsync();

            // Pošaljemo sortiranje opcije u ViewBag
            ViewBag.TimeSortOrder = sortOrder;

            return View(recipes);
        }
 

    // Prikaz forme za dodavanje novog recepta (GET)
    public ActionResult Create()
        {
            return View();
        }

        // Pohrana novog recepta (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Recipe model)
        {
            if (ModelState.IsValid)
            {
                _context.Recipes.Add(model);  // Dodajemo novi recept u bazu
                await _context.SaveChangesAsync();  // Spremamo promjene u bazu

                return RedirectToAction("Index", "Home");  // Nakon što je recept dodan, vraćamo korisnika na Index stranicu
            }

            return View(model);
        }

        // Detalji recepta (GET)
        public async Task<ActionResult> Details(int id)
        {
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound();
            }

            return View(recipe);
        }
    }
}
