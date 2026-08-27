namespace myshop.Domain.Entities.Enums
{
    public enum OrderStatus
    {
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed
    }
}
