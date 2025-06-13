using GastroHub.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<User> Users { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; } // Dodavanje DbSet-a za UserProfile

}