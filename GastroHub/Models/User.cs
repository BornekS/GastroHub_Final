using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GastroHub.Models
{
    public class User
    {
        
        public int Id { get; set; }          // Primarni ključ (ID korisnika)

        [Required]
        [StringLength(100)]
        public string Username { get; set; } // Korisničko ime

        [Required]
        [EmailAddress]
        public string Email { get; set; }    // Email korisnika

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } // Lozinka korisnika // Lozinka korisnika
        public ICollection<Recipe> Recipes { get; set; }  // Kolekcija recepata koje je korisnik dodao
    }
}
