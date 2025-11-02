using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Services;
using AutoMapper;

namespace Nearest.Controllers
{
    /// <summary>
    /// Profil Controller - Kullanıcı profil yönetimi için endpoint'ler sağlar
    /// 
    /// Bu controller, sistemdeki tüm kullanıcı tipleri (Admin ve Company) için 
    /// merkezi profil yönetimini sağlar. JWT token'dan rol bilgisi alarak,
    /// kullanıcı tipine göre ilgili profil bilgilerini döndürür.
    /// 
    /// Yetkilendirme: Tüm endpoint'ler JWT token gerektirir ve sadece 
    /// kendi profil bilgilerine erişim sağlar.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAdminService _adminService;

        public ProfileController(ApplicationDbContext context, IMapper mapper, IAdminService adminService)
        {
            _context = context;
            _mapper = mapper;
            _adminService = adminService;
        }

        /// <summary>
        /// Kullanıcının kendi profil bilgilerini getirir
        /// 
        /// Bu endpoint, JWT token'daki rol bilgisine göre Admin veya Company profilini döndürür.
        /// Kullanıcı sadece kendi bilgilerine erişebilir. Token'dan AdminId veya CompanyId 
        /// bilgisi okunur ve ilgili profil bilgileri döndürülür.
        /// 
        /// Role-based access:
        /// - Admin: AdminDto döndürülür (Id, Email, FirstName, LastName, CreatedAt, IsActive)
        /// - Company: CompanyDto döndürülür (detaylı firma bilgileri, adres, hizmet bölgeleri vb.)
        /// </summary>
        /// <returns>Kullanıcı profil bilgileri</returns>
        /// <response code="200">Profil bilgileri başarıyla döndürüldü</response>
        /// <response code="401">Yetkisiz erişim - Token geçersiz</response>
        /// <response code="403">Yasak - Geçersiz kullanıcı rolü</response>
        /// <response code="404">Bulunamadı - Profil bulunamadı</response>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var role = User.FindFirst("Role")?.Value;
            
            if (role == "Admin")
            {
                var adminIdClaim = User.FindFirst("AdminId")?.Value;
                if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
                {
                    return Unauthorized();
                }

                var admin = await _adminService.GetByIdAsync(adminId);
                if (admin == null)
                {
                    return NotFound();
                }

                return Ok(admin);
            }
            else if (role == "Company")
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (string.IsNullOrEmpty(companyIdClaim) || !int.TryParse(companyIdClaim, out int companyId))
                {
                    return Unauthorized();
                }

                var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == companyId && c.IsActive);
                if (company == null)
                {
                    return NotFound();
                }

                return Ok(_mapper.Map<CompanyDto>(company));
            }
            else
            {
                return Forbid("Geçersiz kullanıcı rolü.");
            }
        }
    }
}

