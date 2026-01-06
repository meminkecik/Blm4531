using Nearest.DTOs;

namespace Nearest.Services
{
    public interface IReviewService
    {
        /// <summary>
        /// Belirli bir çekicinin puan özetini döndürür
        /// </summary>
        Task<RatingSummaryDto> GetRatingSummaryAsync(int towTruckId);

        /// <summary>
        /// Belirli bir çekicinin yorumlarını sayfalı olarak döndürür
        /// </summary>
        Task<TowTruckReviewsResponseDto> GetTowTruckReviewsAsync(int towTruckId, int page = 1, int pageSize = 10);

        /// <summary>
        /// Yeni yorum ekler
        /// </summary>
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto);

        /// <summary>
        /// Belirli bir yorumu siler (Admin)
        /// </summary>
        Task<bool> DeleteReviewAsync(int reviewId);

        /// <summary>
        /// Çekici için ortalama puan ve yorum sayısını hesaplar
        /// </summary>
        Task<(double averageRating, int reviewCount)> GetRatingStatsAsync(int towTruckId);
    }
}
