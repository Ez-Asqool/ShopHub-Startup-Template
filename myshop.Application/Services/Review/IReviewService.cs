using myshop.Application.Services.Review.Dto;
using System.Threading.Tasks;

namespace myshop.Application.Services.Review
{
    public enum ReviewOperationResult
    {
        Success,
        NotFound,
        Forbidden,
        AlreadyReviewed,
        InvalidRating
    }

    public interface IReviewService
    {
        Task<ProductReviewsDto> GetProductReviewsAsync(int productId, string? currentUserId);
        Task<(ReviewOperationResult Status, ReviewDto? Review)> CreateReviewAsync(ReviewCreateDto dto);
        Task<(ReviewOperationResult Status, ReviewDto? Review)> UpdateReviewAsync(int reviewId, string userId, ReviewUpdateDto dto);
        Task<ReviewOperationResult> DeleteReviewAsync(int reviewId, string userId);
    }
}
