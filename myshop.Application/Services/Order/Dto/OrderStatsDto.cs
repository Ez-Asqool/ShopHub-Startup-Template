namespace myshop.Application.Services.Order.Dto
{
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ProcessingCount { get; set; }
        public int DeliveredCount { get; set; }
    }
}
