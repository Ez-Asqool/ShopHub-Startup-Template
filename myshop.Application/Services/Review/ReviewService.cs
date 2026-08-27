using AutoMapper;
using myshop.Application.Contracts;
using myshop.Application.Services.Review.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace myshop.Application.Services.Review
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ReviewService(IReviewRepository reviewRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _reviewRepository = reviewRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ProductReviewsDto> GetProductReviewsAsync(int productId, string? currentUserId)
        {
            var reviews = (await _reviewRepository.GetByProductIdAsync(productId)).ToList();
            var reviewCount = reviews.Count;

            var reviewDtos = reviews.Select(r =>
            {
                var dto = _mapper.Map<ReviewDto>(r);
                dto.IsOwnedByCurrentUser = currentUserId != null && r.ApplicationUserId == currentUserId;
                return dto;
            }).ToList();

            var breakdown = new List<RatingBreakdownDto>();
            for (var stars = 5; stars >= 1; stars--)
            {
                var count = reviews.Count(r => r.Rating == stars);
                breakdown.Add(new RatingBreakdownDto
                {
                    Stars = stars,
                    Count = count,
                    Percentage = reviewCount > 0 ? (int)System.Math.Round(count * 100.0 / reviewCount) : 0
                });
            }

            return new ProductReviewsDto
            {
                AverageRating = reviewCount > 0 ? reviews.Average(r => r.Rating) : 0,
                ReviewCount = reviewCount,
                Breakdown = breakdown,
                Reviews = reviewDtos,
                CurrentUserReview = reviewDtos.FirstOrDefault(r => r.IsOwnedByCurrentUser)
            };
        }

        public async Task<(ReviewOperationResult Status, ReviewDto? Review)> CreateReviewAsync(ReviewCreateDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return (ReviewOperationResult.InvalidRating, null);

            var existing = await _reviewRepository.GetByProductAndUserAsync(dto.ProductId, dto.ApplicationUserId);
            if (existing != null)
                return (ReviewOperationResult.AlreadyReviewed, null);

            var review = _mapper.Map<Domain.Entities.Review>(dto);
            await _reviewRepository.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            var reviewDto = _mapper.Map<ReviewDto>(review);
            reviewDto.IsOwnedByCurrentUser = true;
            return (ReviewOperationResult.Success, reviewDto);
        }

        public async Task<(ReviewOperationResult Status, ReviewDto? Review)> UpdateReviewAsync(int reviewId, string userId, ReviewUpdateDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return (ReviewOperationResult.InvalidRating, null);

            var existing = await _reviewRepository.GetByIdAsync(reviewId);
            if (existing == null)
                return (ReviewOperationResult.NotFound, null);

            if (existing.ApplicationUserId != userId)
                return (ReviewOperationResult.Forbidden, null);

            existing.Rating = dto.Rating;
            existing.Comment = dto.Comment;
            _reviewRepository.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            var reviewDto = _mapper.Map<ReviewDto>(existing);
            reviewDto.IsOwnedByCurrentUser = true;
            return (ReviewOperationResult.Success, reviewDto);
        }

        public async Task<ReviewOperationResult> DeleteReviewAsync(int reviewId, string userId)
        {
            var existing = await _reviewRepository.GetByIdAsync(reviewId);
            if (existing == null)
                return ReviewOperationResult.NotFound;

            if (existing.ApplicationUserId != userId)
                return ReviewOperationResult.Forbidden;

            _reviewRepository.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
            return ReviewOperationResult.Success;
        }
    }
}
