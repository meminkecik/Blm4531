using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs.TowTruck
{
	// [FromForm] binding ile AreasJson alanı JSON dizesi olarak gönderilecek
	public class UpdateTowTruckDto
	{
		[MaxLength(100)]
		public string? DriverName { get; set; }

		// Örn: [{"ProvinceId":34,"DistrictId":1},{"ProvinceId":34,"DistrictId":2}]
		public string? AreasJson { get; set; }
		
		// Çekicinin aktif/pasif durumu
		public bool? IsActive { get; set; }
	}
}