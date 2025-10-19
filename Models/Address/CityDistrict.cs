using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nearest.Models.Address
{
    public class CityDistrict
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(City))]
        public string CityId { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(District))]
        public string DistrictId { get; set; } = string.Empty;

        // Navigation properties
        public virtual City City { get; set; } = null!;
        public virtual District District { get; set; } = null!;
    }
}
