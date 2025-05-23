using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;

namespace REST_VECINDAPP.Servicios
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var emailConfig = _config.GetSection("EmailSettings");
                var message = new MimeMessage();

                message.From.Add(new MailboxAddress(emailConfig["SenderName"], emailConfig["SenderEmail"]));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var builder = new BodyBuilder();
                if (isHtml)
                    builder.HtmlBody = body;
                else
                    builder.TextBody = body;

                message.Body = builder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    // Conectar al servidor SMTP
                    await client.ConnectAsync(emailConfig["MailServer"],
                                             int.Parse(emailConfig["MailPort"]),
                                             SecureSocketOptions.StartTls);

                    // Autenticar
                    await client.AuthenticateAsync(emailConfig["Username"], emailConfig["Password"]);

                    // Enviar correo
                    await client.SendAsync(message);

                    // Desconectar
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Correo enviado exitosamente a {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al enviar correo: {ex.Message}");
                return false;
            }
        }
    }
}