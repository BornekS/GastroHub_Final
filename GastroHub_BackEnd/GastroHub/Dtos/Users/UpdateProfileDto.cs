using System.ComponentModel.DataAnnotations;

namespace GastroHub.Dtos.Users
{
    public class UpdateProfileDto
    {
        [MaxLength(100)]
        public string? DisplayName { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }
    }
}