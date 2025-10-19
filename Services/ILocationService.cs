using Nearest.DTOs;

namespace Nearest.Services
{
    public interface ILocationService
    {
        Task<List<CompanyDto>> GetNearestCompaniesAsync(double latitude, double longitude, int limit = 10);
        Task<List<CompanyDto>> GetAllCompaniesAsync();
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    }
}
