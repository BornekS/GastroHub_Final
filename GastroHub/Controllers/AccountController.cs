using GastroHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GastroHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Registracija (GET)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Registracija (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Provjera postoji li korisnik s istim korisničkim imenom ili emailom
                if (_context.Users.Any(u => u.Username == user.Username || u.Email == user.Email))
                {
                    ModelState.AddModelError("", "Korisničko ime ili email već postoji.");
                    return View(user);
                }

                _context.Add(user);
                await _context.SaveChangesAsync();

                // Pohranjivanje korisničkog imena u Session
                HttpContext.Session.SetString("User", user.Username);

                // Pohranjivanje korisničkog imena u ViewData za prikaz u Layout
                ViewData["User"] = user.Username;

                return RedirectToAction("Index", "Home");
            }

            return View(user);
        }

        // Prijava (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Prijava (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                // Pohranjivanje korisničkog imena u Session
                HttpContext.Session.SetString("User", user.Username);

                // Pohranjivanje korisničkog imena u ViewData za prikaz u Layout
                ViewData["User"] = user.Username;

                return RedirectToAction("Index", "Home"); // Preusmjerenje na početnu stranicu nakon prijave
            }

            ModelState.AddModelError("", "Pogrešan korisnički podaci.");
            return View();
        }

        // Odjava (POST)
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User"); // Uklanjanje korisničkog imena iz sesije
            return RedirectToAction("Index", "Home");
        }

        // Osobni podaci korisnika (GET)
        [HttpGet]
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return View(user);
        }

        // Moji recepti (GET)
        [HttpGet]
        public IActionResult MyRecipes()
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user != null)
            {
                var recipes = _context.Recipes.Where(r => r.UserId == user.Id).ToList();
                return View(recipes);
            }

            return RedirectToAction("Login", "Account");
        }

        // Osiguranje da podatke iz Session-a pošaljemo u Layout
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            // Pohranjivanje korisničkog imena iz Session u ViewData
            ViewData["User"] = HttpContext.Session.GetString("User");
        }
        // Dodavanje novog recepta (GET)
        [HttpGet]
        public IActionResult CreateRecipe()
        {
            return View();
        }

        // Dodavanje novog recepta (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRecipe(Recipe recipe)
        {
            // Provjera da li je korisnik prijavljen
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account"); // Ako korisnik nije prijavljen, preusmjerenje na prijavu
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                recipe.UserId = user.Id; // Povezivanje recepta s korisnikom
                _context.Recipes.Add(recipe); // Dodavanje recepta u bazu
                await _context.SaveChangesAsync(); // Spremanje promjena u bazu

                return RedirectToAction("MyRecipes", "Account"); // Preusmjerenje na Moje recepte
            }

            return View(recipe); // Ako nešto nije u redu, vraća korisniku formu
        }
        // Detalji recepta (GET)
        [HttpGet]
        public IActionResult RecipeDetails(int id)
        {
            var recipe = _context.Recipes
                .FirstOrDefault(r => r.Id == id);

            if (recipe != null)
            {
                return View(recipe);
            }

            return RedirectToAction("MyRecipes", "Account"); // Ako recept nije pronađen, preusmjerenje na Moje recepte
        }
    }
}
