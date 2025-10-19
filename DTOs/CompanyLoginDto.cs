using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs
{
    public class CompanyLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
