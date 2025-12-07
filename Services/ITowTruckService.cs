using Microsoft.AspNetCore.Http;
using Nearest.DTOs.TowTruck;

namespace Nearest.Services
{
	public interface ITowTruckService
	{
		// Firma yönetimi için
		Task<TowTruckDto> CreateTowTruckAsync(int companyId, TowTruckCreateDto dto, IFormFile? driverPhoto);
		Task<List<TowTruckDto>> GetTowTrucksByCompanyAsync(int companyId, bool includeInactive = false);
		Task<TowTruckDto> UpdateTowTruckAsync(int companyId, int towTruckId, UpdateTowTruckDto dto, IFormFile? driverPhoto);
		Task<bool> DeactivateTowTruckAsync(int companyId, int towTruckId);
		Task<bool> DeleteTowTruckAsync(int companyId, int towTruckId);
		
		// Public API için (kullanıcılar)
		Task<List<TowTruckDto>> GetAllActiveTowTrucksAsync();
		Task<List<TowTruckDto>> GetNearestTowTrucksAsync(
			double latitude,
			double longitude,
			int limit = 10,
			int? provinceId = null,
			int? districtId = null);
	}
}


