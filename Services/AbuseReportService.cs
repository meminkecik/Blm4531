using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;

namespace Nearest.Services
{
    public class AbuseReportService : IAbuseReportService
    {
        private readonly ApplicationDbContext _context;

        public AbuseReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AbuseReportDto> CreateReportAsync(CreateAbuseReportDto dto)
        {
            // En az bir hedef belirtilmeli
            if (!dto.TowTruckId.HasValue && !dto.CompanyId.HasValue)
            {
                throw new ArgumentException("Rapor için bir çekici veya firma belirtilmelidir");
            }

            // Çekici varsa kontrol et
            if (dto.TowTruckId.HasValue)
            {
                var towTruck = await _context.TowTrucks.FindAsync(dto.TowTruckId.Value);
                if (towTruck == null)
                {
                    throw new ArgumentException("Belirtilen çekici bulunamadı");
                }
            }

            // Firma varsa kontrol et
            if (dto.CompanyId.HasValue)
            {
                var company = await _context.Companies.FindAsync(dto.CompanyId.Value);
                if (company == null)
                {
                    throw new ArgumentException("Belirtilen firma bulunamadı");
                }
            }

            var report = new AbuseReport
            {
                TowTruckId = dto.TowTruckId,
                CompanyId = dto.CompanyId,
                ReportType = dto.ReportType,
                Description = dto.Description,
                ReporterName = dto.ReporterName,
                ReporterPhone = dto.ReporterPhone,
                ReporterEmail = dto.ReporterEmail,
                Status = AbuseReportStatus.Pending
            };

            _context.AbuseReports.Add(report);
            await _context.SaveChangesAsync();

            return await MapToDto(report);
        }

        public async Task<List<AbuseReportDto>> GetAllReportsAsync(AbuseReportStatus? status = null)
        {
            var query = _context.AbuseReports
                .Include(a => a.TowTruck)
                .Include(a => a.Company)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(a => a.Status == status.Value);
            }

            var reports = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var result = new List<AbuseReportDto>();
            foreach (var report in reports)
            {
                result.Add(await MapToDto(report));
            }

            return result;
        }

        public async Task<AbuseReportDto?> GetReportByIdAsync(int reportId)
        {
            var report = await _context.AbuseReports
                .Include(a => a.TowTruck)
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == reportId);

            if (report == null) return null;

            return await MapToDto(report);
        }

        public async Task<AbuseReportDto?> UpdateReportAsync(int reportId, UpdateAbuseReportDto dto, int adminId)
        {
            var report = await _context.AbuseReports
                .Include(a => a.TowTruck)
                .Include(a => a.Company)
                .FirstOrDefaultAsync(a => a.Id == reportId);

            if (report == null) return null;

            report.Status = dto.Status;
            report.AdminNote = dto.AdminNote;
            report.UpdatedAt = DateTime.UtcNow;
            report.ReviewedAt = DateTime.UtcNow;
            report.ReviewedByAdminId = adminId;

            await _context.SaveChangesAsync();

            return await MapToDto(report);
        }

        public async Task<bool> DeleteReportAsync(int reportId)
        {
            var report = await _context.AbuseReports.FindAsync(reportId);
            if (report == null) return false;

            _context.AbuseReports.Remove(report);
            await _context.SaveChangesAsync();
            return true;
        }

        private Task<AbuseReportDto> MapToDto(AbuseReport report)
        {
            var dto = new AbuseReportDto
            {
                Id = report.Id,
                TowTruckId = report.TowTruckId,
                TowTruckInfo = report.TowTruck != null ? $"{report.TowTruck.DriverName} - {report.TowTruck.LicensePlate}" : null,
                CompanyId = report.CompanyId,
                CompanyName = report.Company?.CompanyName,
                ReportType = report.ReportType,
                ReportTypeText = GetReportTypeText(report.ReportType),
                Description = report.Description,
                ReporterName = report.ReporterName,
                Status = report.Status,
                StatusText = GetStatusText(report.Status),
                AdminNote = report.AdminNote,
                CreatedAt = report.CreatedAt,
                ReviewedAt = report.ReviewedAt
            };

            return Task.FromResult(dto);
        }

        private static string GetReportTypeText(AbuseReportType type)
        {
            return type switch
            {
                AbuseReportType.Fraud => "Dolandırıcılık",
                AbuseReportType.PoorService => "Kötü Hizmet",
                AbuseReportType.PriceGouging => "Fiyat Sahtekarlığı",
                AbuseReportType.Harassment => "Taciz/Kötü Davranış",
                AbuseReportType.FalseInformation => "Sahte Bilgi",
                AbuseReportType.Other => "Diğer",
                _ => "Bilinmiyor"
            };
        }

        private static string GetStatusText(AbuseReportStatus status)
        {
            return status switch
            {
                AbuseReportStatus.Pending => "Beklemede",
                AbuseReportStatus.UnderReview => "İnceleniyor",
                AbuseReportStatus.Resolved => "Çözüldü",
                AbuseReportStatus.Rejected => "Reddedildi",
                AbuseReportStatus.Archived => "Arşivlendi",
                _ => "Bilinmiyor"
            };
        }
    }
}
