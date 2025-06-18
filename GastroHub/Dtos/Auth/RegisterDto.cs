using System.ComponentModel.DataAnnotations;

namespace GastroHub.Dtos.Auth
{
    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required, Compare("Password")]
        public string ConfirmPassword { get; set; }

        public string? DisplayName { get; set; }
    }
}