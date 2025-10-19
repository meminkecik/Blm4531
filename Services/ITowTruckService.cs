using Microsoft.AspNetCore.Http;
using Nearest.DTOs.TowTruck;

namespace Nearest.Services
{
	public interface ITowTruckService
	{
		Task<TowTruckDto> CreateTowTruckAsync(int companyId, TowTruckCreateDto dto, IFormFile? driverPhoto);
		Task<List<TowTruckDto>> GetTowTrucksByCompanyAsync(int companyId);
	}
}


