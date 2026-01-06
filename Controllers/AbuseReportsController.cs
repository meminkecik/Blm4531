using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.Models;
using Nearest.Services;
using System.Security.Claims;

namespace Nearest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AbuseReportsController : ControllerBase
    {
        private readonly IAbuseReportService _abuseReportService;

        public AbuseReportsController(IAbuseReportService abuseReportService)
        {
            _abuseReportService = abuseReportService;
        }

        /// <summary>
        /// Kötüye kullanım raporu oluşturur
        /// </summary>
        /// <param name="dto">Rapor bilgileri</param>
        /// <returns>Oluşturulan rapor</returns>
        /// <response code="201">Rapor başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek</response>
        [HttpPost]
        public async Task<ActionResult<AbuseReportDto>> CreateReport([FromBody] CreateAbuseReportDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var report = await _abuseReportService.CreateReportAsync(dto);
                return CreatedAtAction(nameof(GetReport), new { reportId = report.Id }, report);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Tüm raporları listeler (Admin)
        /// </summary>
        /// <param name="status">Durum filtresi (opsiyonel)</param>
        /// <returns>Raporlar listesi</returns>
        /// <response code="200">Raporlar başarıyla döndürüldü</response>
        /// <response code="401">Yetkisiz erişim</response>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<List<AbuseReportDto>>> GetAllReports([FromQuery] AbuseReportStatus? status = null)
        {
            // Admin kontrolü
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "Admin" && roleClaim != "SuperAdmin")
            {
                return Unauthorized(new { message = "Bu işlem için admin yetkisi gereklidir" });
            }

            var reports = await _abuseReportService.GetAllReportsAsync(status);
            return Ok(reports);
        }

        /// <summary>
        /// Belirli bir raporu getirir (Admin)
        /// </summary>
        /// <param name="reportId">Rapor ID'si</param>
        /// <returns>Rapor detayları</returns>
        /// <response code="200">Rapor başarıyla döndürüldü</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="404">Rapor bulunamadı</response>
        [Authorize]
        [HttpGet("{reportId}")]
        public async Task<ActionResult<AbuseReportDto>> GetReport(int reportId)
        {
            // Admin kontrolü
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "Admin" && roleClaim != "SuperAdmin")
            {
                return Unauthorized(new { message = "Bu işlem için admin yetkisi gereklidir" });
            }

            var report = await _abuseReportService.GetReportByIdAsync(reportId);
            if (report == null)
            {
                return NotFound(new { message = "Rapor bulunamadı" });
            }

            return Ok(report);
        }

        /// <summary>
        /// Rapor durumunu günceller (Admin)
        /// </summary>
        /// <param name="reportId">Rapor ID'si</param>
        /// <param name="dto">Güncelleme bilgileri</param>
        /// <returns>Güncellenmiş rapor</returns>
        /// <response code="200">Rapor başarıyla güncellendi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="404">Rapor bulunamadı</response>
        [Authorize]
        [HttpPut("{reportId}")]
        public async Task<ActionResult<AbuseReportDto>> UpdateReport(int reportId, [FromBody] UpdateAbuseReportDto dto)
        {
            // Admin kontrolü
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "Admin" && roleClaim != "SuperAdmin")
            {
                return Unauthorized(new { message = "Bu işlem için admin yetkisi gereklidir" });
            }

            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out var adminId))
            {
                return Unauthorized(new { message = "Admin ID alınamadı" });
            }

            var report = await _abuseReportService.UpdateReportAsync(reportId, dto, adminId);
            if (report == null)
            {
                return NotFound(new { message = "Rapor bulunamadı" });
            }

            return Ok(report);
        }

        /// <summary>
        /// Raporu siler (Admin)
        /// </summary>
        /// <param name="reportId">Rapor ID'si</param>
        /// <returns>Silme sonucu</returns>
        /// <response code="200">Rapor başarıyla silindi</response>
        /// <response code="401">Yetkisiz erişim</response>
        /// <response code="404">Rapor bulunamadı</response>
        [Authorize]
        [HttpDelete("{reportId}")]
        public async Task<ActionResult> DeleteReport(int reportId)
        {
            // Admin kontrolü
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim != "Admin" && roleClaim != "SuperAdmin")
            {
                return Unauthorized(new { message = "Bu işlem için admin yetkisi gereklidir" });
            }

            var result = await _abuseReportService.DeleteReportAsync(reportId);
            if (!result)
            {
                return NotFound(new { message = "Rapor bulunamadı" });
            }

            return Ok(new { message = "Rapor başarıyla silindi" });
        }
    }
}
