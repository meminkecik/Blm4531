using System.ComponentModel.DataAnnotations;

namespace Nearest.Models
{
    public class UserLocation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? SessionId { get; set; } // Kullanıcı kayıt olmadan da konum paylaşabilir
    }
}
