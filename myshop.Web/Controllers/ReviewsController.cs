using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using myshop.Application.Services.Review;
using myshop.Application.Services.Review.Dto;
using myshop.Infrastructure.Identity;

namespace myshop.Web.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(IReviewService reviewService, UserManager<ApplicationUser> userManager)
        {
            _reviewService = reviewService;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetForProduct(int productId)
        {
            var currentUserId = _userManager.GetUserId(User);
            var reviews = await _reviewService.GetProductReviewsAsync(productId, currentUserId);
            return Json(reviews);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int productId, int rating, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new { success = false, message = "You must be logged in to post a review." });

            var dto = new ReviewCreateDto
            {
                ProductId = productId,
                ApplicationUserId = user.Id,
                ReviewerName = user.Name,
                Rating = rating,
                Comment = comment
            };

            var (status, review) = await _reviewService.CreateReviewAsync(dto);

            return status switch
            {
                ReviewOperationResult.Success => Json(new { success = true, review }),
                ReviewOperationResult.AlreadyReviewed => Json(new { success = false, message = "You've already reviewed this product." }),
                ReviewOperationResult.InvalidRating => Json(new { success = false, message = "Choose a rating between 1 and 5 stars." }),
                _ => Json(new { success = false, message = "Could not post your review. Please try again." })
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, int rating, string comment)
        {
            var userId = _userManager.GetUserId(User)!;
            var dto = new ReviewUpdateDto { Id = id, Rating = rating, Comment = comment };

            var (status, review) = await _reviewService.UpdateReviewAsync(id, userId, dto);

            return status switch
            {
                ReviewOperationResult.Success => Json(new { success = true, review }),
                ReviewOperationResult.NotFound => Json(new { success = false, message = "Review not found." }),
                ReviewOperationResult.Forbidden => Json(new { success = false, message = "You can only edit your own review." }),
                ReviewOperationResult.InvalidRating => Json(new { success = false, message = "Choose a rating between 1 and 5 stars." }),
                _ => Json(new { success = false, message = "Could not update your review. Please try again." })
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var status = await _reviewService.DeleteReviewAsync(id, userId);

            return status switch
            {
                ReviewOperationResult.Success => Json(new { success = true }),
                ReviewOperationResult.NotFound => Json(new { success = false, message = "Review not found." }),
                ReviewOperationResult.Forbidden => Json(new { success = false, message = "You can only delete your own review." }),
                _ => Json(new { success = false, message = "Could not delete your review. Please try again." })
            };
        }
    }
}
