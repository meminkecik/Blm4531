using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs.Address;
using Nearest.Services;

namespace Nearest.Controllers
{
    /// <summary>
    /// Adres Controller - Türkiye il ve ilçe bilgilerini sağlar
    /// 
    /// Bu controller, Türkiye'nin tüm illerini ve ilçelerini döndüren
    /// endpoint'ler sağlar. Adres bilgileri turkiyeapi.dev API'den
    /// senkronize edilmektedir.
    /// 
    /// Kullanım:
    /// - Kayıt formlarında il/ilçe seçimi
    /// - Arama ve filtreleme işlemleri
    /// - Adres doğrulama
    /// 
    /// Güncelleme: Adres verilerinin güncellenmesi sadece Admin tarafından
    /// AdminController üzerinden yapılır.
    /// </summary>
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
        /// Türkiye'deki tüm illeri listeler
        /// 
        /// Bu endpoint, kayıtlı tüm illeri döndürür (81 il).
        /// Her il, ID ve isim bilgisi içerir.
        /// 
        /// Yanıt formatı:
        /// - Status: İşlem durumu (SUCCESS/ERROR)
        /// - Data: İl listesi (ProvinceId, CityName)
        /// </summary>
        /// <returns>İl listesi (81 il)</returns>
        /// <response code="200">İller başarıyla döndürüldü</response>
        [HttpGet("cities")]
        public async Task<ActionResult<CityResponseDto>> GetCities()
        {
            var result = await _addressService.GetCitiesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Belirtilen ile ait ilçeleri listeler
        /// 
        /// Bu endpoint, verilen il ID'sine ait tüm ilçeleri döndürür.
        /// Her ilçe, ID ve isim bilgisi içerir.
        /// 
        /// Yanıt formatı:
        /// - Status: İşlem durumu (SUCCESS/ERROR)
        /// - Data: İlçe listesi (DistrictId, DistrictName)
        /// 
        /// Örnek: /api/address/districts/34 → İstanbul'un tüm ilçelerini döndürür
        /// </summary>
        /// <param name="provinceId">İl ID'si (örn: 34=İstanbul)</param>
        /// <returns>İlçe listesi</returns>
        /// <response code="200">İlçeler başarıyla döndürüldü</response>
        /// <response code="200">İl bulunamadı (boş liste döner)</response>
        [HttpGet("districts/{provinceId}")]
        public async Task<ActionResult<DistrictResponseDto>> GetDistrictsByCityId(int provinceId)
        {
            var result = await _addressService.GetDistrictsByCityIdAsync(provinceId);
            return Ok(result);
        }

        // Güncelleme işlemi sadece AdminController üzerinden yapılır
    }
}
