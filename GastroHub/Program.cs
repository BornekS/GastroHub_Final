using Microsoft.EntityFrameworkCore;
using GastroHub;

var builder = WebApplication.CreateBuilder(args);

// Dodajemo DbContext za SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dodajemo podršku za Session
builder.Services.AddDistributedMemoryCache(); // Za pohranu podataka u memoriji
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Trajanje sesije
    options.Cookie.HttpOnly = true;  // Sigurnost
    options.Cookie.IsEssential = true; // Obavezno za aplikaciju
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Postavljanje Session middleware-a
app.UseSession();

// Postavljanje putanje za SQLite bazu
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
