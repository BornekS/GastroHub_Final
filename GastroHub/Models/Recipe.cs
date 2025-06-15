using System.ComponentModel.DataAnnotations;

namespace GastroHub.Models
{
    public class Recipe
    {
        public int Id { get; set; }           // Primarni ključ (ID recepta)
        public string Name { get; set; }      // Naziv recepta
        public string Ingredients { get; set; }// Sastojci recepta
        public string Instructions { get; set; }// Upute za pripremu recepta
        public int UserId { get; set; }
       

        [Required]
        public string PreparationTime { get; set; }
        
        public bool IsFavorite { get; set; }

        public User User { get; set; }

        public string Category { get; set; }
    }
}