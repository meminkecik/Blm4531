using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs.TowTruck;
using Nearest.Services;

namespace Nearest.Controllers
{
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
		/// Kullanıcının konumuna göre en yakın çekicileri bulur (Kademeli Arama)
		/// 
		/// Bu endpoint şu şekilde çalışır:
		/// 1. Öncelikle kullanıcının bulunduğu ilçede hizmet veren çekiciler listelenir
		/// 2. Ardından aynı ildeki diğer ilçelerdeki çekiciler mesafeye göre eklenir
		/// 3. Daha sonra komşu illerdeki çekiciler mesafeye göre eklenir
		/// 4. Son olarak tüm Türkiye'deki çekiciler mesafeye göre sıralanır
		/// 
		/// Her adımda limit dolana kadar devam edilir.
		/// Konum izni verilmemişse rastgele 20 çekici döndürülür.
		/// 
		/// Limit: 1-50 arası çekici döndürülebilir (varsayılan: 20).
		/// </summary>
		/// <param name="latitude">Kullanıcının enlem bilgisi (örn: 41.0082)</param>
		/// <param name="longitude">Kullanıcının boylam bilgisi (örn: 29.0094)</param>
		/// <param name="provinceId">Kullanıcının bulunduğu ilin ID'si (opsiyonel)</param>
		/// <param name="districtId">Kullanıcının bulunduğu ilçenin ID'si (opsiyonel)</param>
		/// <param name="limit">Döndürülecek maksimum çekici sayısı (varsayılan: 20)</param>
		/// <returns>Filtrelenmiş en yakın çekiciler listesi (mesafeye göre sıralı)</returns>
		/// <response code="200">Çekiciler başarıyla döndürüldü</response>
		/// <response code="400">Limit 1-50 arasında olmalıdır</response>
		[HttpGet("nearest")]
		public async Task<ActionResult<List<TowTruckDto>>> GetNearestTowTrucks(
			[FromQuery] double latitude = 0,
			[FromQuery] double longitude = 0,
			[FromQuery] int limit = 20,
			[FromQuery] int? provinceId = null,
			[FromQuery] int? districtId = null)
		{
			if (limit <= 0 || limit > 50)
			{
				return BadRequest("Limit 1-50 arasında olmalıdır.");
			}

			// Geçerli koordinatlar varsa ve il/ilçe bilgisi yoksa, koordinatlardan tespit et
			var hasValidCoordinates = latitude != 0 && longitude != 0 
			                          && !double.IsNaN(latitude) && !double.IsNaN(longitude);
			
			if (hasValidCoordinates && (!provinceId.HasValue || !districtId.HasValue))
			{
				var locationInfo = await _locationService.GetProvinceAndDistrictFromCoordinatesAsync(latitude, longitude);
				provinceId ??= locationInfo.provinceId;
				districtId ??= locationInfo.districtId;
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
