using Microsoft.EntityFrameworkCore;
using System.Linq;
using GastroHub.Models;
using GastroHub;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestroHub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Prikaz svih recepata
        public async Task<IActionResult> Index(string searchTerm, string ingredientFilter)
        {
            // Dohvati sve recepte ili filtriraj prema searchTerm
            var recipesQuery = _context.Recipes.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                recipesQuery = recipesQuery.Where(r => r.Name.Contains(searchTerm) || r.Ingredients.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(ingredientFilter))
            {
                recipesQuery = recipesQuery.Where(r => r.Ingredients.Contains(ingredientFilter));
            }

            var recipes = await recipesQuery.ToListAsync();

            // Dohvati sve sastojke iz recepata, razdvojene zarezom, nakon što su podaci dohvaćeni
            var ingredients = recipes
                .Where(r => !string.IsNullOrEmpty(r.Ingredients)) // Filtriraj samo one koji imaju sastojke
                .SelectMany(r => r.Ingredients.Split(',')) // Razdvoji sastojke po zarezu
                .Distinct() // Uzmi jedinstvene sastojke
                .ToList(); // Pohrani ih u listu

            // Pošaljite listu sastojaka u ViewBag za filtriranje
            ViewBag.IngredientFilter = new SelectList(ingredients);

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
