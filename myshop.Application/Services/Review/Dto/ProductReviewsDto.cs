using System.Collections.Generic;

namespace myshop.Application.Services.Review.Dto
{
    public class ProductReviewsDto
    {
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<RatingBreakdownDto> Breakdown { get; set; } = new();
        public List<ReviewDto> Reviews { get; set; } = new();
        public ReviewDto? CurrentUserReview { get; set; }
    }
}
