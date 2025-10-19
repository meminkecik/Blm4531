using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs.TowTruck
{
	// [FromForm] binding ile AreasJson alanı JSON dizesi olarak gönderilecek
	public class TowTruckCreateDto
	{
		[Required]
		[MaxLength(20)]
		public string LicensePlate { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		public string DriverName { get; set; } = string.Empty;

		// Örn: [{"ProvinceId":34,"DistrictId":1},{"ProvinceId":34,"DistrictId":2}]
		[Required]
		public string AreasJson { get; set; } = string.Empty;
	}
}


