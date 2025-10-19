using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nearest.Models.Address
{
    public class City
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public int ProvinceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CityName { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<CityDistrict> Districts { get; set; } = new List<CityDistrict>();
    }
}
