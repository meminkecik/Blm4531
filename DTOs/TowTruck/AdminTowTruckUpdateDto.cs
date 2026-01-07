using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs.TowTruck
{
    public class AdminTowTruckUpdateDto
    {
        public string? DriverName { get; set; }
        public string? AreasJson { get; set; }
        public bool? IsActive { get; set; }
        public string? LicensePlate { get; set; }
        public int? CompanyId { get; set; }
    }
}
