using System.ComponentModel.DataAnnotations;

namespace Nearest.Models
{
    /// <summary>
    /// Kullanıcıların çekici/şoför hakkında bıraktığı yorumlar
    /// </summary>
    public class Review
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Yorum yapılan çekici
        /// </summary>
        [Required]
        public int TowTruckId { get; set; }
        public TowTruck TowTruck { get; set; } = null!;

        /// <summary>
        /// Yorum yapan kullanıcının adı (anonim olabilir)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string ReviewerName { get; set; } = string.Empty;

        /// <summary>
        /// Yorum yapan kullanıcının telefon numarası (doğrulama için)
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string ReviewerPhone { get; set; } = string.Empty;

        /// <summary>
        /// Puan (1-5 arası)
        /// </summary>
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        /// <summary>
        /// Yorum metni
        /// </summary>
        [MaxLength(1000)]
        public string? Comment { get; set; }

        /// <summary>
        /// Yorum tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Yorum onaylandı mı? (Admin onayı gerekebilir)
        /// </summary>
        public bool IsApproved { get; set; } = true;

        /// <summary>
        /// Yorum görünür mü?
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
