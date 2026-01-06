using Nearest.DTOs;
using Nearest.Models;

namespace Nearest.Services
{
    public interface IAbuseReportService
    {
        /// <summary>
        /// Yeni kötüye kullanım raporu oluşturur
        /// </summary>
        Task<AbuseReportDto> CreateReportAsync(CreateAbuseReportDto dto);

        /// <summary>
        /// Tüm raporları listeler (Admin)
        /// </summary>
        Task<List<AbuseReportDto>> GetAllReportsAsync(AbuseReportStatus? status = null);

        /// <summary>
        /// Belirli bir raporu getirir (Admin)
        /// </summary>
        Task<AbuseReportDto?> GetReportByIdAsync(int reportId);

        /// <summary>
        /// Rapor durumunu günceller (Admin)
        /// </summary>
        Task<AbuseReportDto?> UpdateReportAsync(int reportId, UpdateAbuseReportDto dto, int adminId);

        /// <summary>
        /// Raporu siler (Admin)
        /// </summary>
        Task<bool> DeleteReportAsync(int reportId);
    }
}
