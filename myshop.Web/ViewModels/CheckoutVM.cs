using myshop.Application.Models;
using System.ComponentModel.DataAnnotations;

namespace myshop.Web.ViewModels
{
    public class CheckoutVM
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }

        [Required(ErrorMessage = "Recipient name is required.")]
        [Display(Name = "Recipient Name")]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }
}
