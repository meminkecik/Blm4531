using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs.Address;
using Nearest.Services;

namespace Nearest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        /// <summary>
        /// Tüm illeri listeler
        /// </summary>
        /// <returns>İl listesi</returns>
        [HttpGet("cities")]
        public async Task<ActionResult<CityResponseDto>> GetCities()
        {
            var result = await _addressService.GetCitiesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Belirtilen ile ait ilçeleri listeler
        /// </summary>
        /// <param name="provinceId">İl ID'si</param>
        /// <returns>İlçe listesi</returns>
        [HttpGet("districts/{provinceId}")]
        public async Task<ActionResult<DistrictResponseDto>> GetDistrictsByCityId(int provinceId)
        {
            var result = await _addressService.GetDistrictsByCityIdAsync(provinceId);
            return Ok(result);
        }

        // Güncelleme işlemi sadece AdminController üzerinden yapılır
    }
}
