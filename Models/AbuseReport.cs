using System.ComponentModel.DataAnnotations;

namespace Nearest.Models
{
    /// <summary>
    /// Kötüye kullanım raporları
    /// </summary>
    public class AbuseReport
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Rapor edilen çekici (opsiyonel)
        /// </summary>
        public int? TowTruckId { get; set; }
        public TowTruck? TowTruck { get; set; }

        /// <summary>
        /// Rapor edilen firma (opsiyonel)
        /// </summary>
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }

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
        public string ReporterPhone { get; set; } = string.Empty;

        /// <summary>
        /// Rapor eden kullanıcının e-postası (opsiyonel)
        /// </summary>
        [MaxLength(100)]
        public string? ReporterEmail { get; set; }

        /// <summary>
        /// Rapor durumu
        /// </summary>
        public AbuseReportStatus Status { get; set; } = AbuseReportStatus.Pending;

        /// <summary>
        /// Admin notu (inceleme sonucu)
        /// </summary>
        [MaxLength(1000)]
        public string? AdminNote { get; set; }

        /// <summary>
        /// Rapor tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Son güncelleme tarihi
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// İnceleme tarihi
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// İnceleyen admin ID'si
        /// </summary>
        public int? ReviewedByAdminId { get; set; }
        public Admin? ReviewedByAdmin { get; set; }
    }

    /// <summary>
    /// Kötüye kullanım rapor türleri
    /// </summary>
    public enum AbuseReportType
    {
        /// <summary>Dolandırıcılık</summary>
        Fraud = 1,
        /// <summary>Kötü hizmet</summary>
        PoorService = 2,
        /// <summary>Fiyat sahtekarlığı</summary>
        PriceGouging = 3,
        /// <summary>Taciz/Kötü davranış</summary>
        Harassment = 4,
        /// <summary>Sahte bilgi</summary>
        FalseInformation = 5,
        /// <summary>Diğer</summary>
        Other = 99
    }

    /// <summary>
    /// Rapor durumları
    /// </summary>
    public enum AbuseReportStatus
    {
        /// <summary>Beklemede</summary>
        Pending = 0,
        /// <summary>İnceleniyor</summary>
        UnderReview = 1,
        /// <summary>Onaylandı/İşlem yapıldı</summary>
        Resolved = 2,
        /// <summary>Reddedildi</summary>
        Rejected = 3,
        /// <summary>Arşivlendi</summary>
        Archived = 4
    }
}
