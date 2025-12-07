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
		private readonly ILocationService _locationService;

		public TowTruckService(ApplicationDbContext context, IMapper mapper, IAddressService addressService, IWebHostEnvironment env, ILocationService locationService)
		{
			_context = context;
			_mapper = mapper;
			_addressService = addressService;
			_env = env;
			_locationService = locationService;
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

		public async Task<List<TowTruckDto>> GetTowTrucksByCompanyAsync(int companyId, bool includeInactive = false)
		{
			var query = _context.TowTrucks
				.Where(t => t.CompanyId == companyId);

			if (!includeInactive)
			{
				query = query.Where(t => t.IsActive);
			}

			var list = await query
				.Include(t => t.OperatingAreas)
				.OrderByDescending(t => t.IsActive)
				.ThenByDescending(t => t.UpdatedAt)
				.ToListAsync();

			return _mapper.Map<List<TowTruckDto>>(list);
		}
		
		public async Task<TowTruckDto> UpdateTowTruckAsync(int companyId, int towTruckId, UpdateTowTruckDto dto, IFormFile? driverPhoto)
		{
			// Çekiciyi bul ve firma sahibi mi kontrol et
			var towTruck = await _context.TowTrucks
				.Include(t => t.OperatingAreas)
				.FirstOrDefaultAsync(t => t.Id == towTruckId && t.CompanyId == companyId);
				
			if (towTruck == null)
			{
				throw new InvalidOperationException("Çekici bulunamadı veya bu firmaya ait değil.");
			}
			
			// Sadece gönderilen alanları güncelle
			if (!string.IsNullOrWhiteSpace(dto.DriverName))
			{
				towTruck.DriverName = dto.DriverName.Trim();
			}
			
			// IsActive alanını güncelle (eğer gönderilmişse)
			if (dto.IsActive.HasValue)
			{
				towTruck.IsActive = dto.IsActive.Value;
			}
			
			// Şoför fotoğrafını güncelle (eğer gönderilmişse)
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
			
			// Çalışma bölgelerini güncelle (eğer gönderilmişse)
			if (!string.IsNullOrWhiteSpace(dto.AreasJson))
			{
				// Mevcut bölgeleri temizle
				_context.TowTruckAreas.RemoveRange(towTruck.OperatingAreas);
				
				// Yeni bölgeleri ekle
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
			}
			
			// Güncelleme zamanını ayarla
			towTruck.UpdatedAt = DateTime.UtcNow;
			
			await _context.SaveChangesAsync();
			return _mapper.Map<TowTruckDto>(towTruck);
		}
		
		public async Task<bool> DeactivateTowTruckAsync(int companyId, int towTruckId)
		{
			// Çekiciyi bul ve firma sahibi mi kontrol et
			var towTruck = await _context.TowTrucks
				.FirstOrDefaultAsync(t => t.Id == towTruckId && t.CompanyId == companyId);
				
			if (towTruck == null)
			{
				return false;
			}
			
			// Pasif yap
			towTruck.IsActive = false;
			towTruck.UpdatedAt = DateTime.UtcNow;
			
			await _context.SaveChangesAsync();
			return true;
		}
		
		public async Task<bool> DeleteTowTruckAsync(int companyId, int towTruckId)
		{
			// Çekiciyi bul ve firma sahibi mi kontrol et
			var towTruck = await _context.TowTrucks
				.Include(t => t.OperatingAreas)
				.FirstOrDefaultAsync(t => t.Id == towTruckId && t.CompanyId == companyId);
				
			if (towTruck == null)
			{
				return false;
			}
			
			// Önce çalışma bölgelerini sil
			_context.TowTruckAreas.RemoveRange(towTruck.OperatingAreas);
			
			// Sonra çekiciyi sil
			_context.TowTrucks.Remove(towTruck);
			
			await _context.SaveChangesAsync();
			return true;
		}
		
		// ============== PUBLIC API METODLARI ==============
		
		public async Task<List<TowTruckDto>> GetAllActiveTowTrucksAsync()
		{
			var towTrucks = await _context.TowTrucks
				.Where(t => t.IsActive)
				.Include(t => t.Company)
				.Include(t => t.OperatingAreas)
				.OrderByDescending(t => t.UpdatedAt)
				.ToListAsync();
				
			return _mapper.Map<List<TowTruckDto>>(towTrucks);
		}
		
		public async Task<List<TowTruckDto>> GetNearestTowTrucksAsync(
			double latitude,
			double longitude,
			int limit = 10,
			int? provinceId = null,
			int? districtId = null)
		{
			var baseQuery = _context.TowTrucks
				.Where(t => t.IsActive)
				.Include(t => t.Company)
				.Include(t => t.OperatingAreas);

			List<TowTruck> towTrucks = new List<TowTruck>();

			// Adım 1: İlçeye göre filtrele
			if (districtId.HasValue)
			{
				towTrucks = await baseQuery
					.Where(t => t.OperatingAreas.Any(a => a.DistrictId == districtId.Value))
					.ToListAsync();

				// Adım 2: İlçede çekici yoksa, aynı ildeki diğer ilçelere bak
				if (!towTrucks.Any() && provinceId.HasValue)
				{
					towTrucks = await baseQuery
						.Where(t => t.OperatingAreas.Any(a => a.ProvinceId == provinceId.Value))
						.ToListAsync();
				}
			}

			// Adım 3: Hala çekici yoksa, ile göre filtrele
			if (!towTrucks.Any() && provinceId.HasValue)
			{
				towTrucks = await baseQuery
					.Where(t => t.OperatingAreas.Any(a => a.ProvinceId == provinceId.Value))
					.ToListAsync();
			}

			// Adım 4: Hala çekici yoksa, tüm aktif çekicileri getir
			if (!towTrucks.Any())
			{
				towTrucks = await baseQuery.ToListAsync();
			}

			var towTruckDtos = _mapper.Map<List<TowTruckDto>>(towTrucks);

			// Mesafe hesaplama
			var hasGeoPoint = !double.IsNaN(latitude) && !double.IsNaN(longitude);
			if (hasGeoPoint)
			{
				foreach (var towTruck in towTruckDtos)
				{
					// Firmanın konumunu kullan
					var company = await _context.Companies
						.FirstOrDefaultAsync(c => c.Id == towTruck.CompanyId);
						
					if (company?.Latitude != null && company?.Longitude != null)
					{
						towTruck.Distance = _locationService.CalculateDistance(
							latitude,
							longitude,
							company.Latitude.Value,
							company.Longitude.Value);
					}
					else
					{
						towTruck.Distance = null;
					}
				}

				towTruckDtos = towTruckDtos
					.OrderBy(t => t.Distance ?? double.MaxValue)
					.Take(limit)
					.ToList();
			}
			else
			{
				towTruckDtos = towTruckDtos
					.Take(limit)
					.ToList();
			}

			return towTruckDtos;
		}
	}
}


