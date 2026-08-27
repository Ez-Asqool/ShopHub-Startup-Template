using myshop.Application.Services.Order.Dto;
using System.Threading.Tasks;

namespace myshop.Application.Contracts
{
    public interface IEmailService
    {
        Task SendWelcomeEmailAsync(string toEmail, string recipientName, string shopUrl);
        Task SendOrderConfirmationEmailAsync(string toEmail, OrderDetailsDto order, string orderUrl, string myOrdersUrl);
    }
}
