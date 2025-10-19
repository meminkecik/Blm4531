using System.ComponentModel.DataAnnotations;

namespace Nearest.Models
{
	public class TowTruck
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public int CompanyId { get; set; }
		public Company Company { get; set; } = null!;

		[Required]
		[MaxLength(20)]
		public string LicensePlate { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		public string DriverName { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? DriverPhotoUrl { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public bool IsActive { get; set; } = true;

		public virtual ICollection<TowTruckArea> OperatingAreas { get; set; } = new List<TowTruckArea>();
	}
}


