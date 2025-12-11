using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Nearest.DTOs.TowTruck
{
	public class TowTruckAreaInputDto
	{
		[Required]
		[JsonPropertyName("provinceId")]
		public int ProvinceId { get; set; }

		[Required]
		[JsonPropertyName("districtId")]
		public int DistrictId { get; set; }
	}
}


