using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
		private readonly ILogger<TowTruckService> _logger;

		public TowTruckService(
			ApplicationDbContext context, 
			IMapper mapper, 
			IAddressService addressService, 
			IWebHostEnvironment env, 
			ILocationService locationService,
			ILogger<TowTruckService> logger)
		{
			_context = context;
			_mapper = mapper;
			_addressService = addressService;
			_env = env;
			_locationService = locationService;
			_logger = logger;
		}

		/// <summary>
		/// Çekici için puan ve yorum sayısı bilgisini getirir
		/// </summary>
		private async Task<(double averageRating, int reviewCount)> GetRatingStatsAsync(int towTruckId)
		{
			var reviews = await _context.Reviews
				.Where(r => r.TowTruckId == towTruckId && r.IsVisible && r.IsApproved)
				.ToListAsync();

			if (!reviews.Any())
			{
				return (0, 0);
			}

			var averageRating = Math.Round(reviews.Average(r => r.Rating), 1);
			return (averageRating, reviews.Count);
		}

		/// <summary>
		/// TowTruckDto'ya puan bilgilerini ekler
		/// </summary>
		private async Task<TowTruckDto> AddRatingInfoAsync(TowTruckDto dto)
		{
			var (averageRating, reviewCount) = await GetRatingStatsAsync(dto.Id);
			dto.AverageRating = averageRating;
			dto.ReviewCount = reviewCount;
			return dto;
		}

		/// <summary>
		/// TowTruckDto listesine puan bilgilerini ekler
		/// </summary>
		private async Task<List<TowTruckDto>> AddRatingInfoAsync(List<TowTruckDto> dtos)
		{
			foreach (var dto in dtos)
			{
				await AddRatingInfoAsync(dto);
			}
			return dtos;
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

		// Company ve OperatingAreas'ı yükle
		await _context.Entry(towTruck).Reference(t => t.Company).LoadAsync();
		await _context.Entry(towTruck).Collection(t => t.OperatingAreas).LoadAsync();
		
		var result = _mapper.Map<TowTruckDto>(towTruck);
		return await AddRatingInfoAsync(result);
	}		public async Task<List<TowTruckDto>> GetTowTrucksByCompanyAsync(int companyId, bool includeInactive = false)
		{
			var query = _context.TowTrucks
				.Where(t => t.CompanyId == companyId);

		if (!includeInactive)
		{
			query = query.Where(t => t.IsActive);
		}

		var list = await query
			.Include(t => t.Company)
			.Include(t => t.OperatingAreas)
			.OrderByDescending(t => t.IsActive)
			.ThenByDescending(t => t.UpdatedAt)
			.ToListAsync();

		var result = _mapper.Map<List<TowTruckDto>>(list);
		return await AddRatingInfoAsync(result);
	}		public async Task<TowTruckDto> UpdateTowTruckAsync(int companyId, int towTruckId, UpdateTowTruckDto dto, IFormFile? driverPhoto)
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
			var result = _mapper.Map<TowTruckDto>(towTruck);
			return await AddRatingInfoAsync(result);
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
				
			var result = _mapper.Map<List<TowTruckDto>>(towTrucks);
			return await AddRatingInfoAsync(result);
		}
		
		/// <summary>
		/// En yakın çekicileri getirir - Kademeli arama algoritması
		/// 
		/// Algoritma Sırası:
		/// 1. Kullanıcının ilçesinde hizmet veren çekiciler
		/// 2. Aynı ildeki diğer ilçelerde hizmet verenler (mesafeye göre sıralı)
		/// 3. Komşu illerdeki çekiciler (mesafeye göre sıralı)
		/// 4. Tüm Türkiye'den en yakın çekiciler
		/// 
		/// Her adımda limit dolana kadar devam eder.
		/// </summary>
		public async Task<List<TowTruckDto>> GetNearestTowTrucksAsync(
			double latitude,
			double longitude,
			int limit = 20,
			int? provinceId = null,
			int? districtId = null)
		{
			var result = new List<TowTruckDto>();
			var addedTowTruckIds = new HashSet<int>(); // Tekrar eklemeyi önlemek için
			
			var hasGeoPoint = !double.IsNaN(latitude) && !double.IsNaN(longitude) 
			                  && latitude != 0 && longitude != 0;

			// ============ ADIM 1: Kullanıcının ilçesindeki çekiciler ============
			if (districtId.HasValue && result.Count < limit)
			{
				var districtTowTrucks = await _context.TowTrucks
					.Where(t => t.IsActive)
					.Where(t => t.OperatingAreas.Any(a => a.DistrictId == districtId.Value))
					.Include(t => t.Company)
					.Include(t => t.OperatingAreas)
					.ToListAsync();

				var dtos = MapAndCalculateDistance(districtTowTrucks, latitude, longitude, hasGeoPoint);
				AddToResult(result, dtos, addedTowTruckIds, limit);
				
				_logger.LogInformation($"Adım 1 - İlçe ({districtId}): {districtTowTrucks.Count} çekici bulundu, toplam: {result.Count}");
			}

			// ============ ADIM 2: Aynı ildeki diğer ilçelerdeki çekiciler ============
			if (provinceId.HasValue && result.Count < limit)
			{
				var provinceTowTrucks = await _context.TowTrucks
					.Where(t => t.IsActive)
					.Where(t => !addedTowTruckIds.Contains(t.Id)) // Zaten eklenmişleri hariç tut
					.Where(t => t.OperatingAreas.Any(a => a.ProvinceId == provinceId.Value))
					.Include(t => t.Company)
					.Include(t => t.OperatingAreas)
					.ToListAsync();

				var dtos = MapAndCalculateDistance(provinceTowTrucks, latitude, longitude, hasGeoPoint);
				
				// Mesafeye göre sırala
				if (hasGeoPoint)
				{
					dtos = dtos.OrderBy(d => d.Distance ?? double.MaxValue).ToList();
				}
				
				AddToResult(result, dtos, addedTowTruckIds, limit);
				
				_logger.LogInformation($"Adım 2 - İl ({provinceId}): {provinceTowTrucks.Count} çekici bulundu, toplam: {result.Count}");
			}

			// ============ ADIM 3: Komşu illerdeki çekiciler ============
			if (hasGeoPoint && result.Count < limit)
			{
				// Tüm illerdeki çekicileri getir ve mesafeye göre sırala
				// Ancak zaten eklenmiş olanları hariç tut
				var allOtherTowTrucks = await _context.TowTrucks
					.Where(t => t.IsActive)
					.Where(t => !addedTowTruckIds.Contains(t.Id))
					.Include(t => t.Company)
					.Include(t => t.OperatingAreas)
					.ToListAsync();

				var dtos = MapAndCalculateDistance(allOtherTowTrucks, latitude, longitude, hasGeoPoint);
				
				// Mesafeye göre sırala - en yakından en uzağa
				dtos = dtos.OrderBy(d => d.Distance ?? double.MaxValue).ToList();
				
				AddToResult(result, dtos, addedTowTruckIds, limit);
				
				_logger.LogInformation($"Adım 3 - Tüm Türkiye: {allOtherTowTrucks.Count} çekici bulundu, toplam: {result.Count}");
			}

			// ============ ADIM 4: Konum yoksa rastgele çekiciler ============
			if (!hasGeoPoint && result.Count < limit)
			{
				var randomTowTrucks = await _context.TowTrucks
					.Where(t => t.IsActive)
					.Where(t => !addedTowTruckIds.Contains(t.Id))
					.Include(t => t.Company)
					.Include(t => t.OperatingAreas)
					.OrderBy(t => Guid.NewGuid()) // Rastgele sıralama
					.Take(limit - result.Count)
					.ToListAsync();

				var dtos = _mapper.Map<List<TowTruckDto>>(randomTowTrucks);
				AddToResult(result, dtos, addedTowTruckIds, limit);
				
				_logger.LogInformation($"Adım 4 - Rastgele: {randomTowTrucks.Count} çekici eklendi, toplam: {result.Count}");
			}

			// Puan bilgilerini ekle
			return await AddRatingInfoAsync(result);
		}

		/// <summary>
		/// TowTruck listesini DTO'ya çevirir ve mesafe hesaplar
		/// </summary>
		private List<TowTruckDto> MapAndCalculateDistance(List<TowTruck> towTrucks, double latitude, double longitude, bool hasGeoPoint)
		{
			return towTrucks.Select(tt => {
				var dto = _mapper.Map<TowTruckDto>(tt);
				
				if (hasGeoPoint && tt.Company?.Latitude != null && tt.Company?.Longitude != null)
				{
					dto.Distance = _locationService.CalculateDistance(
						latitude,
						longitude,
						tt.Company.Latitude.Value,
						tt.Company.Longitude.Value);
				}
				
				return dto;
			}).ToList();
		}

		/// <summary>
		/// DTO listesini sonuç listesine ekler (limit'e kadar)
		/// </summary>
		private void AddToResult(List<TowTruckDto> result, List<TowTruckDto> toAdd, HashSet<int> addedIds, int limit)
		{
			foreach (var dto in toAdd)
			{
				if (result.Count >= limit) break;
				
				if (!addedIds.Contains(dto.Id))
				{
					result.Add(dto);
					addedIds.Add(dto.Id);
				}
			}
		}

		// ========== ADMIN METODLARI ==========

		/// <summary>
		/// Admin için tüm çekicileri listeler (aktif/pasif hepsi)
		/// </summary>
		public async Task<List<TowTruckDto>> GetAllTowTrucksForAdminAsync()
		{
			var towTrucks = await _context.TowTrucks
				.Include(t => t.Company)
				.Include(t => t.OperatingAreas)
				.OrderByDescending(t => t.CreatedAt)
				.ToListAsync();

			var result = _mapper.Map<List<TowTruckDto>>(towTrucks);
			return await AddRatingInfoAsync(result);
		}

		/// <summary>
		/// Admin tarafından çekici güncelleme
		/// </summary>
		public async Task<ServiceResult<TowTruckDto>> UpdateTowTruckByAdminAsync(int towTruckId, AdminTowTruckUpdateDto dto)
		{
			var towTruck = await _context.TowTrucks
				.Include(t => t.Company)
				.Include(t => t.OperatingAreas)
				.FirstOrDefaultAsync(t => t.Id == towTruckId);

			if (towTruck == null)
			{
				return ServiceResult<TowTruckDto>.NotFound("Çekici bulunamadı.");
			}

			// Sadece gönderilen alanları güncelle
			if (!string.IsNullOrEmpty(dto.DriverName))
				towTruck.DriverName = dto.DriverName.Trim();

			if (!string.IsNullOrEmpty(dto.LicensePlate))
				towTruck.LicensePlate = dto.LicensePlate.Trim().ToUpperInvariant();

			if (dto.IsActive.HasValue)
				towTruck.IsActive = dto.IsActive.Value;

			if (dto.CompanyId.HasValue)
			{
				// Yeni firma var mı kontrol et
				var companyExists = await _context.Companies.AnyAsync(c => c.Id == dto.CompanyId.Value);
				if (!companyExists)
				{
					return ServiceResult<TowTruckDto>.Fail("Belirtilen firma bulunamadı.");
				}
				towTruck.CompanyId = dto.CompanyId.Value;
			}

			towTruck.UpdatedAt = DateTime.UtcNow;

			await _context.SaveChangesAsync();

			// Company bilgisini yeniden yükle
			await _context.Entry(towTruck).Reference(t => t.Company).LoadAsync();

			var result = _mapper.Map<TowTruckDto>(towTruck);
			return ServiceResult<TowTruckDto>.Ok(await AddRatingInfoAsync(result));
		}

		/// <summary>
		/// Admin tarafından çekici silme
		/// </summary>
		public async Task<ServiceResult<bool>> DeleteTowTruckByAdminAsync(int towTruckId)
		{
			var towTruck = await _context.TowTrucks
				.Include(t => t.OperatingAreas)
				.FirstOrDefaultAsync(t => t.Id == towTruckId);

			if (towTruck == null)
			{
				return ServiceResult<bool>.NotFound("Çekici bulunamadı.");
			}

			// İlişkili alanları sil
			if (towTruck.OperatingAreas?.Any() == true)
			{
				_context.TowTruckAreas.RemoveRange(towTruck.OperatingAreas);
			}

			_context.TowTrucks.Remove(towTruck);
			await _context.SaveChangesAsync();

			return ServiceResult<bool>.Ok(true);
		}
	}
}


