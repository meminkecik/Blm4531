using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs.TowTruck;
using Nearest.Services;

namespace Nearest.Controllers
{
	/// <summary>
	/// Çekici/Tow Truck Controller - Firma çekici yönetimi
	/// 
	/// Bu controller, firmaların kendi çekici araçlarını yönetmesini sağlar.
	/// Her çekici bir plaka numarası, şoför ve çalışma bölgeleri ile tanımlanır.
	/// 
	/// Yetkilendirme: Sadece Company rolüne sahip kullanıcılar erişebilir.
	/// Her firma sadece kendi çekicilerini yönetebilir.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class TowTrucksController : ControllerBase
	{
		private readonly ITowTruckService _towTruckService;

		public TowTrucksController(ITowTruckService towTruckService)
		{
			_towTruckService = towTruckService;
		}

		/// <summary>
		/// Yeni çekici kaydı oluşturur
		/// 
		/// Bu endpoint, firmanın yeni bir çekici aracını sisteme eklemesini sağlar.
		/// Her çekici için benzersiz plaka numarası gereklidir.
		/// 
		/// Gerekli bilgiler:
		/// - Plaka numarası (unique, sistem genelinde)
		/// - Şoför adı
		/// - Çalışma bölgeleri (JSON formatında il-ilçe listesi)
		/// - Şoför fotoğrafı (opsiyonel, max 20MB)
		/// 
		/// Çalışma bölgeleri: İl ID ve ilçe ID çiftlerinden oluşur.
		/// Bölge isimleri adres servisi ile otomatik çözümlenir.
		/// </summary>
		/// <param name="dto">Çekici kayıt bilgileri</param>
		/// <param name="driverPhoto">Şoför fotoğrafı (opsiyonel)</param>
		/// <returns>Oluşturulmuş çekici bilgileri</returns>
		/// <response code="200">Çekici başarıyla oluşturuldu</response>
		/// <response code="400">Gerekli alanlar eksik veya plaka zaten kayıtlı</response>
		/// <response code="401">Yetkisiz erişim</response>
		/// <response code="403">Sadece Company rolü yetkilidir</response>
		[HttpPost]
		[Authorize]
		[RequestSizeLimit(20_000_000)]
		public async Task<ActionResult<TowTruckDto>> Create([FromForm] TowTruckCreateDto dto, IFormFile? driverPhoto)
		{
			var role = User.FindFirst("Role")?.Value;
			if (role != "Company") return Forbid();
			var companyIdClaim = User.FindFirst("CompanyId")?.Value;
			if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
				return Unauthorized();

			// Alan doğrulamaları
			if (string.IsNullOrWhiteSpace(dto.LicensePlate)) return BadRequest("Plaka zorunludur.");
			if (string.IsNullOrWhiteSpace(dto.DriverName)) return BadRequest("Şoför adı zorunludur.");
			if (string.IsNullOrWhiteSpace(dto.AreasJson)) return BadRequest("Çalışma bölgeleri zorunludur.");

			var created = await _towTruckService.CreateTowTruckAsync(companyId, dto, driverPhoto);
			return Ok(created);
		}

		/// <summary>
		/// Firma kullanıcısının kendi çekicilerini listeler
		/// 
		/// Bu endpoint, firmanın sahip olduğu tüm aktif çekicileri
		/// çalışma bölgeleri ile birlikte döndürür.
		/// 
		/// Dönen bilgiler:
		/// - Çekici bilgileri (plaka, şoför, fotoğraf)
		/// - Her bir çekicinin çalışma bölgeleri
		/// - Oluşturulma tarihi
		/// </summary>
		/// <returns>Firmanın çekici listesi</returns>
		/// <response code="200">Çekici listesi başarıyla döndürüldü</response>
		/// <response code="401">Yetkisiz erişim</response>
		/// <response code="403">Sadece Company rolü yetkilidir</response>
		[HttpGet("my")]
		[Authorize]
		public async Task<ActionResult<List<TowTruckDto>>> GetMy()
		{
			var role = User.FindFirst("Role")?.Value;
			if (role != "Company") return Forbid();
			var companyIdClaim = User.FindFirst("CompanyId")?.Value;
			if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
				return Unauthorized();

			var list = await _towTruckService.GetTowTrucksByCompanyAsync(companyId);
			return Ok(list);
		}
	}
}


