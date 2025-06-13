namespace GastroHub.Models
{
    public class UserProfile
    {
        public int Id { get; set; }    // Primarni ključ (ID)
        public string FullName { get; set; }  // Ime i Prezime korisnika
        public string Email { get; set; }     // Email korisnika

        // Veza s User modelom (1:1)
        public int UserId { get; set; }
        public User User { get; set; } // Navigacijsko svojstvo
    }
}
