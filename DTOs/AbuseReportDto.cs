using System.ComponentModel.DataAnnotations;
using Nearest.Models;

namespace Nearest.DTOs
{
    /// <summary>
    /// Kötüye kullanım raporu listesi için DTO
    /// </summary>
    public class AbuseReportDto
    {
        public int Id { get; set; }
        public int? TowTruckId { get; set; }
        public string? TowTruckInfo { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public AbuseReportType ReportType { get; set; }
        public string ReportTypeText { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReporterName { get; set; } = string.Empty;
        public AbuseReportStatus Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    /// <summary>
    /// Kötüye kullanım raporu oluşturma için DTO
    /// </summary>
    public class CreateAbuseReportDto
    {
        /// <summary>
        /// Rapor edilen çekici ID (opsiyonel)
        /// </summary>
        public int? TowTruckId { get; set; }

        /// <summary>
        /// Rapor edilen firma ID (opsiyonel)
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// Rapor türü
        /// </summary>
        [Required]
        public AbuseReportType ReportType { get; set; }

        /// <summary>
        /// Rapor açıklaması
        /// </summary>
        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Rapor eden kullanıcının adı
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ReporterName { get; set; } = string.Empty;

        /// <summary>
        /// Rapor eden kullanıcının telefonu
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Phone]
        public string ReporterPhone { get; set; } = string.Empty;

        /// <summary>
        /// Rapor eden kullanıcının e-postası (opsiyonel)
        /// </summary>
        [MaxLength(100)]
        [EmailAddress]
        public string? ReporterEmail { get; set; }
    }

    /// <summary>
    /// Admin için rapor güncelleme DTO
    /// </summary>
    public class UpdateAbuseReportDto
    {
        [Required]
        public AbuseReportStatus Status { get; set; }

        [MaxLength(1000)]
        public string? AdminNote { get; set; }
    }
}
