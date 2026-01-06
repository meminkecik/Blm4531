using System.ComponentModel.DataAnnotations;

namespace Nearest.DTOs
{
    /// <summary>
    /// Yorum listesi için DTO
    /// </summary>
    public class ReviewDto
    {
        public int Id { get; set; }
        public int TowTruckId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Yorum ekleme için DTO
    /// </summary>
    public class CreateReviewDto
    {
        [Required]
        public int TowTruckId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReviewerName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Phone]
        public string ReviewerPhone { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
    }

    /// <summary>
    /// Çekici için puan özeti
    /// </summary>
    public class RatingSummaryDto
    {
        /// <summary>
        /// Ortalama puan (5 üzerinden)
        /// </summary>
        public double AverageRating { get; set; }

        /// <summary>
        /// Toplam yorum sayısı
        /// </summary>
        public int ReviewCount { get; set; }
    }

    /// <summary>
    /// Çekicinin yorumları için sayfalı response
    /// </summary>
    public class TowTruckReviewsResponseDto
    {
        public int TowTruckId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public RatingSummaryDto RatingSummary { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
