using System.Text.Json;
using Nearest.DTOs.Address;
using Nearest.Models.Address;
using Nearest.Repositories;

namespace Nearest.Services
{
    public class AddressHelperService : IAddressHelperService
    {
        private readonly ICityRepository _cityRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly ICityDistrictRepository _cityDistrictRepository;
        private readonly HttpClient _httpClient;

        public AddressHelperService(
            ICityRepository cityRepository,
            IDistrictRepository districtRepository,
            ICityDistrictRepository cityDistrictRepository,
            HttpClient httpClient)
        {
            _cityRepository = cityRepository;
            _districtRepository = districtRepository;
            _cityDistrictRepository = cityDistrictRepository;
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message)> FetchRemoteAddressAsync()
        {
            const string url = "https://turkiyeapi.dev/api/v1/provinces";
            
            try
            {
                var response = await _httpClient.GetStringAsync(url);
                if (string.IsNullOrEmpty(response))
                {
                    return (false, "Response is null or empty");
                }

                var provincesResponse = JsonSerializer.Deserialize<ProvincesResponseDto>(response);
                if (provincesResponse?.Data == null)
                {
                    return (false, "Data is null");
                }

                var provinces = provincesResponse.Data;

                // Şehirleri kaydet/güncelle
                await UpdateOrSaveCitiesAsync(provinces);

                // İlçeleri kaydet/güncelle
                await UpdateOrSaveDistrictsAsync(provinces);

                // Şehir-İlçe ilişkilerini kaydet/güncelle
                await UpdateOrSaveCityDistrictsAsync(provinces);

                return (true, "Address data fetched successfully");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task UpdateOrSaveCitiesAsync(List<ProvincesCityDataDto> provinces)
        {
            foreach (var province in provinces)
            {
                var existingCity = await _cityRepository.GetByProvinceIdAsync(province.Id);
                
                if (existingCity == null)
                {
                    var city = new City
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProvinceId = province.Id,
                        CityName = province.Name
                    };
                    await _cityRepository.AddAsync(city);
                }
                else
                {
                    existingCity.CityName = province.Name;
                    await _cityRepository.UpdateAsync(existingCity);
                }
            }
        }

        private async Task UpdateOrSaveDistrictsAsync(List<ProvincesCityDataDto> provinces)
        {
            foreach (var province in provinces)
            {
                foreach (var districtData in province.Districts)
                {
                    var existingDistrict = await _districtRepository.GetByDistrictIdAsync(districtData.Id);
                    
                    if (existingDistrict == null)
                    {
                        var district = new District
                        {
                            Id = Guid.NewGuid().ToString(),
                            DistrictId = districtData.Id,
                            DistrictName = districtData.Name
                        };
                        await _districtRepository.AddAsync(district);
                    }
                    else
                    {
                        existingDistrict.DistrictName = districtData.Name;
                        await _districtRepository.UpdateAsync(existingDistrict);
                    }
                }
            }
        }

        private async Task UpdateOrSaveCityDistrictsAsync(List<ProvincesCityDataDto> provinces)
        {
            foreach (var province in provinces)
            {
                var city = await _cityRepository.GetByProvinceIdAsync(province.Id);
                if (city == null) continue;

                foreach (var districtData in province.Districts)
                {
                    var district = await _districtRepository.GetByDistrictIdAsync(districtData.Id);
                    if (district == null) continue;

                    var existingCityDistrict = await _cityDistrictRepository.GetByCityAndDistrictAsync(city, district);
                    
                    if (existingCityDistrict == null)
                    {
                        var cityDistrict = new CityDistrict
                        {
                            Id = Guid.NewGuid().ToString(),
                            CityId = city.Id,
                            DistrictId = district.Id
                        };
                        await _cityDistrictRepository.AddAsync(cityDistrict);
                    }
                    // Mevcut olanları güncellemeye gerek yok, sadece yeni olanları ekle
                }
            }
        }
    }
}
