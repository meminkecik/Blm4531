using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Services;

namespace Nearest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAddressService _addressService;
        private readonly ITicketService _ticketService;

        public AdminController(IAdminService adminService, IAddressService addressService, ITicketService ticketService)
        {
            _adminService = adminService;
            _addressService = addressService;
            _ticketService = ticketService;
        }

        /// <summary>
        /// Admin giriş
        /// </summary>
        /// <param name="loginDto">Admin giriş bilgileri</param>
        /// <returns>JWT token</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AdminAuthResponseDto>> Login([FromBody] AdminLoginDto loginDto)
        {
            var result = await _adminService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized("Email veya şifre hatalı.");
            }

            return Ok(result);
        }

        /// <summary>
        /// Adres verilerini güncelle
        /// </summary>
        /// <returns>Güncelleme sonucu</returns>
        [HttpPut("address")]
        [Authorize]
        public async Task<ActionResult<string>> UpdateAddress()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin")
            {
                return Forbid("Bu işlem için admin yetkisi gerekli.");
            }

            var result = await _addressService.UpdateAddressAsync();
            return Ok(new { message = result });
        }

        /// <summary>
        /// Tüm ticket'ları listele (Admin)
        /// </summary>
        /// <returns>Ticket listesi</returns>
        [HttpGet("tickets")]
        [Authorize]
        public async Task<ActionResult<List<Ticket>>> GetAllTickets()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin")
            {
                return Forbid("Bu işlem için admin yetkisi gerekli.");
            }

            var tickets = await _ticketService.GetAllTicketsAsync();
            return Ok(tickets);
        }

        /// <summary>
        /// Admin profil bilgileri
        /// </summary>
        /// <returns>Admin bilgileri</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<AdminDto>> GetProfile()
        {
            var role = User.FindFirst("Role")?.Value;
            if (role != "Admin")
            {
                return Forbid("Bu işlem için admin yetkisi gerekli.");
            }

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
    }
}
