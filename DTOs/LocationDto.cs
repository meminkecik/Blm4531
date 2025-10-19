using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs
{
    public class LocationDto
    {
        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [MaxLength(200)]
        public string? Address { get; set; }

        [MaxLength(50)]
        public string? SessionId { get; set; }
    }
}
