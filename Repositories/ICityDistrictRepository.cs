using Nearest.Models.Address;

namespace Nearest.Repositories
{
    public interface ICityDistrictRepository
    {
        Task<IEnumerable<CityDistrict>> GetAllAsync();
        Task<CityDistrict?> GetByCityAndDistrictAsync(City city, District district);
        Task<IEnumerable<CityDistrict>> GetByCityAsync(City city);
        Task<CityDistrict> AddAsync(CityDistrict cityDistrict);
        Task<CityDistrict> UpdateAsync(CityDistrict cityDistrict);
        Task<bool> ExistsAsync(City city, District district);
    }
}
