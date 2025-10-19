using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Services;
using System.Security.Cryptography;
using System.Text;

namespace Nearest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public AuthController(ApplicationDbContext context, IMapper mapper, IJwtService jwtService)
        {
            _context = context;
            _mapper = mapper;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(CompanyRegistrationDto dto)
        {
            // Email zaten var mı kontrol et
            if (await _context.Companies.AnyAsync(c => c.Email == dto.Email))
            {
                return BadRequest("Bu email adresi zaten kullanılıyor.");
            }

            // Telefon numarası zaten var mı kontrol et (unique)
            if (await _context.Companies.AnyAsync(c => c.PhoneNumber == dto.PhoneNumber))
            {
                return BadRequest("Bu telefon numarası zaten kullanılıyor.");
            }

            var company = _mapper.Map<Company>(dto);

            // Province/District isimlerini entegre adres sisteminden resolve et
            if (dto.ProvinceId > 0)
            {
                var cityName = await HttpContext.RequestServices.GetRequiredService<IAddressService>().GetCityNameAsync(dto.ProvinceId);
                if (!string.IsNullOrEmpty(cityName))
                {
                    company.City = cityName;
                    company.ProvinceId = dto.ProvinceId;
                }
            }
            if (dto.DistrictId > 0)
            {
                var districtName = await HttpContext.RequestServices.GetRequiredService<IAddressService>().GetDistrictNameAsync(dto.DistrictId);
                if (!string.IsNullOrEmpty(districtName))
                {
                    company.District = districtName;
                    company.DistrictId = dto.DistrictId;
                }
            }

            company.PasswordHash = HashPassword(dto.Password);

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            var companyDto = _mapper.Map<CompanyDto>(company);
            var token = _jwtService.GenerateToken(company);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Company = companyDto
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(CompanyLoginDto dto)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email == dto.Email && c.IsActive);

            if (company == null || !VerifyPassword(dto.Password, company.PasswordHash))
            {
                return Unauthorized("Email veya şifre hatalı.");
            }

            var companyDto = _mapper.Map<CompanyDto>(company);
            var token = _jwtService.GenerateToken(company);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Company = companyDto
            });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}
