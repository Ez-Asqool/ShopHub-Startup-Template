namespace myshop.Application.Services.Order.Dto
{
    public class CheckoutDto
    {
        public string UserId { get; set; }
        public string RecipientName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
