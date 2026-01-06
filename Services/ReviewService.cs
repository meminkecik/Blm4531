using Microsoft.EntityFrameworkCore;
using Nearest.Data;
using Nearest.DTOs;
using Nearest.Models;

namespace Nearest.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RatingSummaryDto> GetRatingSummaryAsync(int towTruckId)
        {
            var stats = await GetRatingStatsAsync(towTruckId);
            return new RatingSummaryDto
            {
                AverageRating = stats.averageRating,
                ReviewCount = stats.reviewCount
            };
        }

        public async Task<TowTruckReviewsResponseDto> GetTowTruckReviewsAsync(int towTruckId, int page = 1, int pageSize = 10)
        {
            var towTruck = await _context.TowTrucks.FindAsync(towTruckId);
            if (towTruck == null)
            {
                throw new ArgumentException("Çekici bulunamadı");
            }

            var query = _context.Reviews
                .Where(r => r.TowTruckId == towTruckId && r.IsVisible && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync();

            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    TowTruckId = r.TowTruckId,
                    ReviewerName = r.ReviewerName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            var stats = await GetRatingStatsAsync(towTruckId);

            return new TowTruckReviewsResponseDto
            {
                TowTruckId = towTruckId,
                DriverName = towTruck.DriverName,
                RatingSummary = new RatingSummaryDto
                {
                    AverageRating = stats.averageRating,
                    ReviewCount = stats.reviewCount
                },
                Reviews = reviews,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto)
        {
            // Çekicinin var olduğunu kontrol et
            var towTruck = await _context.TowTrucks.FindAsync(dto.TowTruckId);
            if (towTruck == null)
            {
                throw new ArgumentException("Çekici bulunamadı");
            }

            // Aynı telefon numarasıyla aynı çekiciye yorum yapılmış mı kontrol et
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.TowTruckId == dto.TowTruckId && r.ReviewerPhone == dto.ReviewerPhone);

            if (existingReview != null)
            {
                throw new InvalidOperationException("Bu çekici için zaten bir yorum yapmışsınız");
            }

            var review = new Review
            {
                TowTruckId = dto.TowTruckId,
                ReviewerName = dto.ReviewerName,
                ReviewerPhone = dto.ReviewerPhone,
                Rating = dto.Rating,
                Comment = dto.Comment,
                IsApproved = true, // Otomatik onay (isteğe bağlı admin onayı yapılabilir)
                IsVisible = true
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return new ReviewDto
            {
                Id = review.Id,
                TowTruckId = review.TowTruckId,
                ReviewerName = review.ReviewerName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return false;
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(double averageRating, int reviewCount)> GetRatingStatsAsync(int towTruckId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.TowTruckId == towTruckId && r.IsVisible && r.IsApproved)
                .ToListAsync();

            if (!reviews.Any())
            {
                return (0, 0);
            }

            var averageRating = Math.Round(reviews.Average(r => r.Rating), 1);
            return (averageRating, reviews.Count);
        }
    }
}
