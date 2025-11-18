using Nearest.DTOs;

namespace Nearest.Services
{
    public interface ILocationService
    {
        Task<List<CompanyDto>> GetNearestCompaniesAsync(
            double latitude,
            double longitude,
            int limit = 10,
            int? provinceId = null,
            int? districtId = null);
        Task<List<CompanyDto>> GetAllCompaniesAsync();
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
        
        /// <summary>
        /// Gets the province and district IDs from geographic coordinates
        /// </summary>
        /// <param name="latitude">The latitude coordinate</param>
        /// <param name="longitude">The longitude coordinate</param>
        /// <returns>A tuple containing provinceId and districtId</returns>
        Task<(int? provinceId, int? districtId)> GetProvinceAndDistrictFromCoordinatesAsync(double latitude, double longitude);
    }
}
