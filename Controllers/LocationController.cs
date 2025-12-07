using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs.TowTruck;
using Nearest.Services;

namespace Nearest.Controllers
{
	/// <summary>
	/// Public Tow Trucks Controller - Kullanıcıların çekici araması
	/// 
	/// Bu controller, kullanıcıların (misafir veya kayıtlı) konumlarına göre
	/// en yakın çekicileri bulmalarını sağlar.
	/// 
	/// Yetkilendirme: Herkese açık (Authentication gerekmez)
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class LocationController : ControllerBase
	{
		private readonly ITowTruckService _towTruckService;
		private readonly ILocationService _locationService;

		public LocationController(ITowTruckService towTruckService, ILocationService locationService)
		{
			_towTruckService = towTruckService;
			_locationService = locationService;
		}

		/// <summary>
		/// Tüm aktif çekicileri listeler (rastgele sırada)
		/// 
		/// Bu endpoint, site açıldığında konum bilgisi olmadan
		/// tüm aktif çekicileri döndürür.
		/// 
		/// Kullanım: Anasayfa için, konum izni verilmeden önce
		/// </summary>
		/// <returns>Tüm aktif çekiciler listesi</returns>
		/// <response code="200">Çekiciler başarıyla döndürüldü</response>
		[HttpGet]
		public async Task<ActionResult<List<TowTruckDto>>> GetAllTowTrucks()
		{
			var towTrucks = await _towTruckService.GetAllActiveTowTrucksAsync();
			return Ok(towTrucks);
		}

		/// <summary>
		/// Kullanıcının konumuna göre en yakın çekicileri bulur
		/// 
		/// Bu endpoint şu şekilde çalışır:
		/// 1. Kullanıcının konum bilgisinden (latitude/longitude) il ve ilçe bilgisi tespit edilir (opsiyonel)
		/// 2. Öncelikle kullanıcının bulunduğu ilçede çalışan çekiciler listelenir
		/// 3. Eğer ilçede çekici yoksa, aynı ildeki çekiciler listelenir
		/// 4. Eğer ilde çekici yoksa, tüm aktif çekiciler listelenir
		/// 5. Ardından gönderilen koordinatlar (latitude/longitude) üzerinden firma konumlarına göre
		///    mesafe hesaplanır ve en yakın çekiciler döndürülür
		/// 
		/// Limit: 1-50 arası çekici döndürülebilir.
		/// </summary>
		/// <param name="latitude">Kullanıcının enlem bilgisi (örn: 41.0082)</param>
		/// <param name="longitude">Kullanıcının boylam bilgisi (örn: 29.0094)</param>
		/// <param name="provinceId">Kullanıcının bulunduğu ilin ID'si (opsiyonel)</param>
		/// <param name="districtId">Kullanıcının bulunduğu ilçenin ID'si (opsiyonel)</param>
		/// <param name="limit">Döndürülecek maksimum çekici sayısı (varsayılan: 10)</param>
		/// <returns>Filtrelenmiş en yakın çekiciler listesi</returns>
		/// <response code="200">Çekiciler başarıyla döndürüldü</response>
		/// <response code="400">Limit 1-50 arasında olmalıdır</response>
		[HttpGet("nearest")]
		public async Task<ActionResult<List<TowTruckDto>>> GetNearestTowTrucks(
			[FromQuery] double latitude,
			[FromQuery] double longitude,
			[FromQuery] int limit = 10,
			[FromQuery] int? provinceId = null,
			[FromQuery] int? districtId = null)
		{
			if (limit <= 0 || limit > 50)
			{
				return BadRequest("Limit 1-50 arasında olmalıdır.");
			}

			// İl ve ilçe bilgisi verilmemişse, koordinatlardan tespit et
			if (!provinceId.HasValue || !districtId.HasValue)
			{
				var locationInfo = await _locationService.GetProvinceAndDistrictFromCoordinatesAsync(latitude, longitude);
				provinceId = locationInfo.provinceId;
				districtId = locationInfo.districtId;
			}

			var towTrucks = await _towTruckService.GetNearestTowTrucksAsync(
				latitude,
				longitude,
				limit,
				provinceId,
				districtId);

			return Ok(towTrucks);
		}
	}
}
