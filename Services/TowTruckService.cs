using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs.TowTruck;
using Nearest.Models;
using System.Text.Json;

namespace Nearest.Services
{
	public class TowTruckService : ITowTruckService
	{
		private readonly ApplicationDbContext _context;
		private readonly IMapper _mapper;
		private readonly IAddressService _addressService;
		private readonly IWebHostEnvironment _env;

		public TowTruckService(ApplicationDbContext context, IMapper mapper, IAddressService addressService, IWebHostEnvironment env)
		{
			_context = context;
			_mapper = mapper;
			_addressService = addressService;
			_env = env;
		}

		public async Task<TowTruckDto> CreateTowTruckAsync(int companyId, TowTruckCreateDto dto, IFormFile? driverPhoto)
		{
			var companyExists = await _context.Companies.AnyAsync(c => c.Id == companyId && c.IsActive);
			if (!companyExists)
			{
				throw new InvalidOperationException("Firma bulunamadı veya pasif.");
			}

			// Aynı plaka var mı kontrol et
			var normalizedPlate = dto.LicensePlate.Trim().ToUpperInvariant();
			var existsPlate = await _context.TowTrucks.AnyAsync(t => t.LicensePlate == normalizedPlate);
			if (existsPlate)
			{
				throw new InvalidOperationException("Bu plaka zaten kayıtlı.");
			}

			var towTruck = new TowTruck
			{
				CompanyId = companyId,
				LicensePlate = normalizedPlate,
				DriverName = dto.DriverName.Trim(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				IsActive = true
			};

			if (driverPhoto != null && driverPhoto.Length > 0)
			{
				var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "drivers");
				Directory.CreateDirectory(uploadsRoot);
				var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(driverPhoto.FileName)}";
				var fullPath = Path.Combine(uploadsRoot, fileName);
				using (var stream = new FileStream(fullPath, FileMode.Create))
				{
					await driverPhoto.CopyToAsync(stream);
				}
				towTruck.DriverPhotoUrl = $"/uploads/drivers/{fileName}";
			}

			// Areas parse ve isim çözümleme
			var areas = JsonSerializer.Deserialize<List<TowTruckAreaInputDto>>(dto.AreasJson) ?? new List<TowTruckAreaInputDto>();
			foreach (var area in areas)
			{
				var cityName = await _addressService.GetCityNameAsync(area.ProvinceId) ?? string.Empty;
				var districtName = await _addressService.GetDistrictNameAsync(area.DistrictId) ?? string.Empty;
				towTruck.OperatingAreas.Add(new TowTruckArea
				{
					ProvinceId = area.ProvinceId,
					DistrictId = area.DistrictId,
					City = cityName,
					District = districtName
				});
			}

			_context.TowTrucks.Add(towTruck);
			await _context.SaveChangesAsync();

			await _context.Entry(towTruck).Collection(t => t.OperatingAreas).LoadAsync();
			return _mapper.Map<TowTruckDto>(towTruck);
		}

		public async Task<List<TowTruckDto>> GetTowTrucksByCompanyAsync(int companyId)
		{
			var list = await _context.TowTrucks
				.Where(t => t.CompanyId == companyId && t.IsActive)
				.Include(t => t.OperatingAreas)
				.ToListAsync();
			return _mapper.Map<List<TowTruckDto>>(list);
		}
	}
}


