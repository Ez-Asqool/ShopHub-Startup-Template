using System.Text.Json;
using myshop.Application.Contracts;
using myshop.Application.Models;

namespace myshop.Web.Services
{
    public class SessionCartService : ICartService
    {
        private const string CartSessionKey = "Cart";

        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionCartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<CartItem> GetCart()
        {
            var json = Session.GetString(CartSessionKey);

            return string.IsNullOrEmpty(json)
                ? new List<CartItem>()
                : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        public void AddItem(CartItem item)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ProductId == item.ProductId);

            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                cart.Add(item);

            SaveCart(cart);
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == productId);
            SaveCart(cart);
        }

        public void IncreaseQuantity(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null)
                return;

            item.Quantity++;
            SaveCart(cart);
        }

        public void DecreaseQuantity(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item == null)
                return;

            item.Quantity--;
            if (item.Quantity <= 0)
                cart.Remove(item);

            SaveCart(cart);
        }

        public void ClearCart()
        {
            Session.Remove(CartSessionKey);
        }

        private void SaveCart(List<CartItem> cart)
        {
            Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }
    }
}
