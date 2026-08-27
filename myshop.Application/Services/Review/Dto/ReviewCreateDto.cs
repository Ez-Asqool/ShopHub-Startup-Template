namespace myshop.Application.Services.Review.Dto
{
    public class ReviewCreateDto
    {
        public int ProductId { get; set; }
        public string ApplicationUserId { get; set; }
        public string ReviewerName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
