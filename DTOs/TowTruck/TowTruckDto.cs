using System;
using System.Collections.Generic;

namespace Nearest.DTOs.TowTruck
{
	public class TowTruckDto
	{
		public int Id { get; set; }
		public string LicensePlate { get; set; } = string.Empty;
		public string DriverName { get; set; } = string.Empty;
		public string? DriverPhotoUrl { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public List<TowTruckAreaDto> OperatingAreas { get; set; } = new();
		
		// Firma bilgileri (public listing için)
		public int CompanyId { get; set; }
		public string CompanyName { get; set; } = string.Empty;
		public string CompanyPhone { get; set; } = string.Empty;
		public double? Distance { get; set; } // Kullanıcıya olan mesafe (km)

		// Puan ve yorum bilgileri
		public double AverageRating { get; set; } // 5 üzerinden ortalama puan
		public int ReviewCount { get; set; } // Toplam yorum sayısı
	}

	public class TowTruckAreaDto
	{
		public int ProvinceId { get; set; }
		public int DistrictId { get; set; }
		public string City { get; set; } = string.Empty;
		public string District { get; set; } = string.Empty;
	}
}


