// Data/ApplicationDbContext.cs
using GastroHub.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GastroHub.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; }
        public DbSet<RecipeLike> RecipeLikes { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<RecipeMedia> RecipeMedia { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<RecipeLike>()
                .HasKey(rl => new { rl.UserId, rl.RecipeId });

            builder.Entity<RecipeLike>()
                .HasOne(rl => rl.User)
                .WithMany(u => u.LikedRecipes)
                .HasForeignKey(rl => rl.UserId);

            builder.Entity<RecipeLike>()
                .HasOne(rl => rl.Recipe)
                .WithMany(r => r.Likes) 
                .HasForeignKey(rl => rl.RecipeId);

            builder.Entity<Favorite>()
                .HasKey(f => new { f.UserId, f.RecipeId });

            builder.Entity<CommentLike>()
                .HasKey(cl => new { cl.UserId, cl.CommentId });

            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}