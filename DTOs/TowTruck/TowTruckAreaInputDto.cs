using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs.TowTruck
{
	public class TowTruckAreaInputDto
	{
		[Required]
		public int ProvinceId { get; set; }

		[Required]
		public int DistrictId { get; set; }
	}
}


