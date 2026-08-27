using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using myshop.Application.Contracts;
using myshop.Application.Services.Order.Dto;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace myshop.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string recipientName, string shopUrl)
        {
            var body = LoadTemplate("welcome-email.html")
                .Replace("{{RecipientName}}", recipientName)
                .Replace("{{ShopUrl}}", shopUrl);

            await SendAsync(toEmail, "Welcome to ShopHub", body);
        }

        public async Task SendOrderConfirmationEmailAsync(string toEmail, OrderDetailsDto order, string orderUrl, string myOrdersUrl)
        {
            var itemsHtml = new StringBuilder();
            foreach (var item in order.Items)
            {
                itemsHtml.Append(LoadTemplate("order-item-row.html")
                    .Replace("{{ProductName}}", item.ProductName)
                    .Replace("{{Quantity}}", item.Quantity.ToString(CultureInfo.InvariantCulture))
                    .Replace("{{UnitPrice}}", FormatCurrency(item.UnitPrice))
                    .Replace("{{LineTotal}}", FormatCurrency(item.UnitPrice * item.Quantity)));
            }

            var phoneRow = string.IsNullOrWhiteSpace(order.PhoneNumber) ? string.Empty : $"<br />{order.PhoneNumber}";
            var orderCode = $"SH-{order.Id:D5}";

            var body = LoadTemplate("order-confirmation-email.html")
                .Replace("{{OrderCode}}", orderCode)
                .Replace("{{OrderStatus}}", order.OrderStatus.ToString())
                .Replace("{{RecipientName}}", order.RecipientName)
                .Replace("{{Address}}", order.Address)
                .Replace("{{City}}", order.City)
                .Replace("{{PhoneRow}}", phoneRow)
                .Replace("{{PaymentStatus}}", order.PaymentStatus.ToString())
                .Replace("{{ItemCount}}", order.Items.Count.ToString(CultureInfo.InvariantCulture))
                .Replace("{{OrderItemsHtml}}", itemsHtml.ToString())
                .Replace("{{OrderTotal}}", FormatCurrency(order.TotalPrice))
                .Replace("{{OrderUrl}}", orderUrl)
                .Replace("{{MyOrdersUrl}}", myOrdersUrl);

            await SendAsync(toEmail, $"Your ShopHub order {orderCode} is confirmed", body);
        }

        private async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable);

            if (!string.IsNullOrEmpty(_settings.Username))
                await client.AuthenticateAsync(_settings.Username, _settings.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string FormatCurrency(decimal value) => "$" + value.ToString("N2", CultureInfo.InvariantCulture);

        private static string LoadTemplate(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"myshop.Infrastructure.Email.EmailTemplates.{fileName}";

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Embedded email template not found: {resourceName}");
            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
