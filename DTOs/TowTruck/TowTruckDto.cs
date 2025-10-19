namespace Nearest.DTOs.TowTruck
{
	public class TowTruckDto
	{
		public int Id { get; set; }
		public string LicensePlate { get; set; } = string.Empty;
		public string DriverName { get; set; } = string.Empty;
		public string? DriverPhotoUrl { get; set; }
		public List<TowTruckAreaDto> OperatingAreas { get; set; } = new();
	}

	public class TowTruckAreaDto
	{
		public int ProvinceId { get; set; }
		public int DistrictId { get; set; }
		public string City { get; set; } = string.Empty;
		public string District { get; set; } = string.Empty;
	}
}


