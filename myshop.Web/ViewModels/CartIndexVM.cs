namespace myshop.Web.ViewModels
{
    public class CartIndexVM
    {
        public List<CartLineVM> Items { get; set; } = new();
    }

    public class CartLineVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
