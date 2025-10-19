using Nearest.DTOs.Address;

namespace Nearest.Services
{
    public interface IAddressService
    {
        Task<CityResponseDto> GetCitiesAsync();
        Task<DistrictResponseDto> GetDistrictsByCityIdAsync(int provinceId);
        Task<string> UpdateAddressAsync();
        Task<string?> GetCityNameAsync(int id);
        Task<string?> GetDistrictNameAsync(int id);
    }
}
