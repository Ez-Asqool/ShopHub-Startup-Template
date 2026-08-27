using System;

namespace myshop.Application.Services.Review.Dto
{
    public class ReviewDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ReviewerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOwnedByCurrentUser { get; set; }
    }
}
