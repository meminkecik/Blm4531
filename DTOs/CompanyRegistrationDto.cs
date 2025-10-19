using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs
{
    public class CompanyRegistrationDto
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public int ProvinceId { get; set; }

        [Required]
        public int DistrictId { get; set; }

        // Eski alanlar, backward compatibility için tutulur ancak doldurulması gerekmez
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FullAddress { get; set; } = string.Empty;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceCity { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? ServiceDistrict { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
