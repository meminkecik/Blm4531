using System.ComponentModel.DataAnnotations;

namespace Nearest.Models
{
	public class TowTruckArea
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int TowTruckId { get; set; }
		public TowTruck TowTruck { get; set; } = null!;

		[Required]
		public int ProvinceId { get; set; }

		[Required]
		public int DistrictId { get; set; }

		[MaxLength(100)]
		public string City { get; set; } = string.Empty;

		[MaxLength(100)]
		public string District { get; set; } = string.Empty;
	}
}


