using System.ComponentModel.DataAnnotations;

namespace Nearest.Models.Address
{
    public class District
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public int DistrictId { get; set; }

        [Required]
        [MaxLength(100)]
        public string DistrictName { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<CityDistrict> Cities { get; set; } = new List<CityDistrict>();
    }
}
