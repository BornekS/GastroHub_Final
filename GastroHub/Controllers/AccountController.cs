using GastroHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
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
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            // 1. Provjera korisničkog imena (format)
            var usernamePattern = @"^[a-zA-Z0-9_-]+$";
            if (!Regex.IsMatch(username, usernamePattern))
            {
                TempData["Error"] = "Korisničko ime smije sadržavati samo slova, brojeve, - i _ , bez razmaka.";
                return RedirectToAction("Register");
            }

            // 2. Provjera lozinki (moraju biti iste)
            if (password != confirmPassword)
            {
                TempData["Error"] = "Lozinke se ne podudaraju.";
                return RedirectToAction("Register");
            }

            // 3. Provjera postoji li korisnik s istim korisničkim imenom ili emailom u bazi
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
            if (existingUser != null)
            {
                TempData["Error"] = "Korisničko ime ili email već postoji.";
                return RedirectToAction("Register");
            }

            // 4. Dodavanje korisnika u bazu
            var newUser = new User
            {
                Username = username,
                Email = email,
                Password = password // Napomena: Lozinka bi trebala biti pohranjena sigurno (hashirana)
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Registracija uspješna! Možete se prijaviti.";
            return RedirectToAction("Index", "Home");
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

            ModelState.AddModelError("", "Pogrešani korisnički podaci.");
            return View();
        }

        // Odjava (POST)
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("User"); // Uklanjanje korisničkog imena iz sesije
            return RedirectToAction("Index", "Home");
        }

        // Osobni podaci korisnika (GET)
        // Osobni podaci korisnika (GET)
        [HttpGet]
        public IActionResult Profile()
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.Include(u => u.Recipes).FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                // Filtriraj samo omiljene recepte
                user.Recipes = user.Recipes.Where(r => r.IsFavorite).ToList();

                return View(user); // Prikazivanje korisničkih podataka i omiljenih recepata
            }

            return RedirectToAction("Login", "Account"); // Ako korisnik nije prijavljen, preusmjerenje na prijavu
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
        public async Task<IActionResult> CreateRecipe(Recipe recipe, IFormFile imageUrl)
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
               

                recipe.UserId = user.Id; // Povezivanje recepta s korisnikom
                _context.Recipes.Add(recipe);
                await _context.SaveChangesAsync();

                return RedirectToAction("MyRecipes", "Account");
            }

            return View(recipe);
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
        // Prikazivanje forme za uređivanje recepta (GET)
        [HttpGet]
        public IActionResult EditRecipe(int id)
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account"); // Ako korisnik nije prijavljen, preusmjerenje na prijavu
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                var recipe = _context.Recipes.FirstOrDefault(r => r.Id == id && r.UserId == user.Id); // Provjeravamo da je recept korisnika

                if (recipe != null)
                {
                    return View(recipe); // Ako recept postoji, prikazujemo formu za uređivanje
                }
            }

            return RedirectToAction("MyRecipes", "Account"); // Ako recept nije pronađen, preusmjeravanje na Moje recepte
        }
        // Spremanje izmjena recepta (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRecipe(Recipe recipe, IFormFile imageUrl)
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                var existingRecipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == recipe.Id && r.UserId == user.Id);

                if (existingRecipe != null)
                {
                    existingRecipe.Name = recipe.Name;
                    existingRecipe.Ingredients = recipe.Ingredients;
                    existingRecipe.Instructions = recipe.Instructions;
                    existingRecipe.PreparationTime = recipe.PreparationTime;

                  

                    _context.Recipes.Update(existingRecipe);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("MyRecipes", "Account");
                }
            }

            return View(recipe);
        }
        // Brisanje recepta (GET)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

                if (recipe != null)
                {
                    _context.Recipes.Remove(recipe);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("MyRecipes", "Account"); // Nakon brisanja, preusmjerenje na Moje recepte
        }
        // Označi recept kao omiljeni (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsFavorite(int id)
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

                if (recipe != null)
                {
                    // Označavanje recepta kao omiljenog
                    recipe.IsFavorite = true;

                    _context.Recipes.Update(recipe);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("RecipeDetails", "Account", new { id = id }); // Preusmjerenje na detalje recepta
        }

        // Ukloni recept iz omiljenih (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites(int id)
        {
            var username = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

                if (recipe != null)
                {
                    // Uklanjanje recepta iz omiljenih
                    recipe.IsFavorite = false;

                    _context.Recipes.Update(recipe);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("RecipeDetails", "Account", new { id = id }); // Preusmjerenje na detalje recepta
        }
    }

}
