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
		/// Her çekici için benzersiz plaka numarası gereklidir.
		/// Gerekli bilgiler:
		/// - Plaka numarası (unique, sistem genelinde)
		/// - Şoför adı
		/// - Çalışma bölgeleri (JSON formatında il-ilçe listesi)
		/// - Şoför fotoğrafı (opsiyonel, max 20MB)
		/// 
		/// Çalışma bölgeleri: İl ID ve ilçe ID çiftlerinden oluşur.
		/// Bölge isimleri adres servisi ile otomatik çözümlenir.
		/// </summary>
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
		/// Bu endpoint, firmanın sahip olduğu çekicileri çalışma
		/// bölgeleri ile birlikte döndürür.
		/// Varsayılan olarak sadece aktif çekiciler listelenir.
		/// includeInactive=true gönderilirse pasif çekiciler de dahil edilir.
		/// 
		/// Dönen bilgiler:
		/// - Çekici bilgileri (plaka, şoför, fotoğraf)
		/// - Her bir çekicinin çalışma bölgeleri
		/// - Oluşturulma tarihi
		/// </summary>
		[HttpGet("my")]
		[Authorize]
		public async Task<ActionResult<List<TowTruckDto>>> GetMy([FromQuery] bool includeInactive = false)
		{
			var role = User.FindFirst("Role")?.Value;
			if (role != "Company") return Forbid();
			var companyIdClaim = User.FindFirst("CompanyId")?.Value;
			if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
				return Unauthorized();

			var list = await _towTruckService.GetTowTrucksByCompanyAsync(companyId, includeInactive);
			return Ok(list);
		}
		
		/// <summary>
		/// Sadece gönderilen alanlar güncellenir, boş bırakılan alanlar değiştirilmez.
		/// Güncellenebilir bilgiler:
		/// - Şoför adı
		/// - Çalışma bölgeleri (JSON formatında il-ilçe listesi)
		/// - Şoför fotoğrafı (opsiyonel, max 20MB)
		/// - Aktiflik durumu (IsActive)
		/// Plaka numarası değiştirilemez.
		/// </summary>
		[HttpPut("{id}")]
		[Authorize]
		[RequestSizeLimit(20_000_000)]
		public async Task<ActionResult<TowTruckDto>> Update(int id, [FromForm] UpdateTowTruckDto dto, IFormFile? driverPhoto)
		{
			var role = User.FindFirst("Role")?.Value;
			if (role != "Company") return Forbid();
			var companyIdClaim = User.FindFirst("CompanyId")?.Value;
			if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
				return Unauthorized();

			try
			{
				var updated = await _towTruckService.UpdateTowTruckAsync(companyId, id, dto, driverPhoto);
				return Ok(updated);
			}
			catch (InvalidOperationException ex)
			{
				return NotFound(ex.Message);
			}
		}
		
		/// <summary>
		/// Çekiciyi pasif duruma getirir
		/// 
		/// Bu endpoint, firmanın sahip olduğu bir çekiciyi pasif duruma getirmesini sağlar.
		/// Pasif çekiciler, firma listesinde görünmez ve arama sonuçlarında çıkmaz.
		/// </summary>
		[HttpPut("{id}/deactivate")]
		[Authorize]
		public async Task<ActionResult> Deactivate(int id)
		{
			var role = User.FindFirst("Role")?.Value;
			if (role != "Company") return Forbid();
			var companyIdClaim = User.FindFirst("CompanyId")?.Value;
			if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
				return Unauthorized();

			var result = await _towTruckService.DeactivateTowTruckAsync(companyId, id);
			if (!result)
			{
				return NotFound("Çekici bulunamadı veya bu firmaya ait değil.");
			}
			
			return Ok(new { message = "Çekici pasif duruma getirildi." });
		}

		[HttpDelete("{id}")]
		[Authorize]
		public async Task<ActionResult> Delete(int id)
		{
			var role = User.FindFirst("Role")?.Value;
			if (role != "Company") return Forbid();
			var companyIdClaim = User.FindFirst("CompanyId")?.Value;
			if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
				return Unauthorized();

			var result = await _towTruckService.DeleteTowTruckAsync(companyId, id);
			if (!result)
			{
				return NotFound("Çekici bulunamadı veya bu firmaya ait değil.");
			}
			
			return Ok(new { message = "Çekici başarıyla silindi." });
		}
	}
}


