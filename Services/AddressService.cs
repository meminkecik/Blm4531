using AutoMapper;
using Nearest.DTOs.Address;
using Nearest.Models.Address;
using Nearest.Repositories;

namespace Nearest.Services
{
    public class AddressService : IAddressService
    {
        private readonly ICityRepository _cityRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly ICityDistrictRepository _cityDistrictRepository;
        private readonly IAddressHelperService _addressHelperService;
        private readonly IMapper _mapper;

        public AddressService(
            ICityRepository cityRepository,
            IDistrictRepository districtRepository,
            ICityDistrictRepository cityDistrictRepository,
            IAddressHelperService addressHelperService,
            IMapper mapper)
        {
            _cityRepository = cityRepository;
            _districtRepository = districtRepository;
            _cityDistrictRepository = cityDistrictRepository;
            _addressHelperService = addressHelperService;
            _mapper = mapper;
        }

        public async Task<CityResponseDto> GetCitiesAsync()
        {
            var cities = await _cityRepository.GetAllAsync();
            var cityDtos = _mapper.Map<List<CityDto>>(cities);
            
            return new CityResponseDto
            {
                Status = "SUCCESS",
                Data = cityDtos
            };
        }

        public async Task<DistrictResponseDto> GetDistrictsByCityIdAsync(int provinceId)
        {
            var city = await _cityRepository.GetByProvinceIdAsync(provinceId);
            if (city == null)
            {
                return new DistrictResponseDto
                {
                    Status = "ERROR",
                    Data = new List<DistrictDto>()
                };
            }

            var cityDistricts = await _cityDistrictRepository.GetByCityAsync(city);
            var districtDtos = _mapper.Map<List<DistrictDto>>(cityDistricts);

            return new DistrictResponseDto
            {
                Status = "SUCCESS",
                Data = districtDtos
            };
        }

        public async Task<string> UpdateAddressAsync()
        {
            var result = await _addressHelperService.FetchRemoteAddressAsync();
            if (!result.Success)
            {
                throw new Exception($"Adres verileri güncellenirken hata oluştu: {result.Message}");
            }
            return result.Message;
        }

        public async Task<string?> GetCityNameAsync(int id)
        {
            var city = await _cityRepository.GetByProvinceIdAsync(id);
            return city?.CityName;
        }

        public async Task<string?> GetDistrictNameAsync(int id)
        {
            var district = await _districtRepository.GetByDistrictIdAsync(id);
            return district?.DistrictName;
        }
    }
}
