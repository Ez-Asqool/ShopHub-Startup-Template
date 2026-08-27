using myshop.Application.Models;

namespace myshop.Web.ViewModels
{
    public class PaymentVM
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}
