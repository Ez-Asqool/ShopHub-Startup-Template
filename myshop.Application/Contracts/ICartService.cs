using myshop.Application.Models;

namespace myshop.Application.Contracts
{
    public interface ICartService
    {
        List<CartItem> GetCart();
        void AddItem(CartItem item);
        void RemoveItem(int productId);
        void IncreaseQuantity(int productId);
        void DecreaseQuantity(int productId);
        void ClearCart();
    }
}
