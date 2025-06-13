using System.ComponentModel.DataAnnotations;

namespace GastroHub.Models
{
    public class Recipe
    {
        public int Id { get; set; }           // Primarni ključ (ID recepta)
        public string Name { get; set; }      // Naziv recepta
        public string Ingredients { get; set; }// Sastojci recepta
        public string Instructions { get; set; }// Upute za pripremu recepta
        public int UserId { get; set; }      // Strani ključ koji upućuje na korisnika

        [Required]
        public string PreparationTime { get; set; }
        // Navigacijsko svojstvo (korisnik koji je dodao recept)
        // Dodano polje za označavanje omiljenih recepata
        public bool IsFavorite { get; set; } // Označava je li recept omiljeni
    }
}