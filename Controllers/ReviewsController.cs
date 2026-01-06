using Microsoft.AspNetCore.Mvc;
using Nearest.DTOs;
using Nearest.Services;

namespace Nearest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Belirli bir çekicinin yorumlarını listeler
        /// </summary>
        /// <param name="towTruckId">Çekici ID'si</param>
        /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="pageSize">Sayfa başına yorum sayısı (varsayılan: 10, max: 50)</param>
        /// <returns>Çekicinin yorumları ve puan özeti</returns>
        /// <response code="200">Yorumlar başarıyla döndürüldü</response>
        /// <response code="404">Çekici bulunamadı</response>
        [HttpGet("towtruck/{towTruckId}")]
        public async Task<ActionResult<TowTruckReviewsResponseDto>> GetTowTruckReviews(
            int towTruckId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            try
            {
                var reviews = await _reviewService.GetTowTruckReviewsAsync(towTruckId, page, pageSize);
                return Ok(reviews);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Yeni yorum ekler
        /// </summary>
        /// <param name="dto">Yorum bilgileri</param>
        /// <returns>Oluşturulan yorum</returns>
        /// <response code="201">Yorum başarıyla oluşturuldu</response>
        /// <response code="400">Geçersiz istek veya zaten yorum yapılmış</response>
        /// <response code="404">Çekici bulunamadı</response>
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var review = await _reviewService.CreateReviewAsync(dto);
                return CreatedAtAction(nameof(GetTowTruckReviews), new { towTruckId = review.TowTruckId }, review);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
