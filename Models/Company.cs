using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nearest.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

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
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        public int ProvinceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string District { get; set; } = string.Empty;

        [Required]
        public int DistrictId { get; set; }

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
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // KVKK Onay Bilgileri
        /// <summary>
        /// KVKK açık rıza onayı verildi mi?
        /// </summary>
        public bool KvkkConsent { get; set; } = false;

        /// <summary>
        /// KVKK onay tarihi
        /// </summary>
        public DateTime? KvkkConsentDate { get; set; }

        /// <summary>
        /// Onay verilen KVKK metin versiyonu
        /// </summary>
        [MaxLength(20)]
        public string? KvkkConsentVersion { get; set; }

        /// <summary>
        /// Onay verilirken kullanılan IP adresi
        /// </summary>
        [MaxLength(45)]
        public string? KvkkConsentIpAddress { get; set; }

        // Navigation properties
        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
