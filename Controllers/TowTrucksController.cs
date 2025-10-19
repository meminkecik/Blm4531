using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs.TowTruck;
using Nearest.Services;

namespace Nearest.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TowTrucksController : ControllerBase
	{
		private readonly ITowTruckService _towTruckService;

		public TowTrucksController(ITowTruckService towTruckService)
		{
			_towTruckService = towTruckService;
		}

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


