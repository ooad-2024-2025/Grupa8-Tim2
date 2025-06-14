using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net.Security;
//using Microsoft.Extensions.Options;
namespace K_K.Models
{
    public class EmailService : IEmailSender
    {
        private readonly EmailSettings _settings;
        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.From));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            smtp.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => {
                return errors == SslPolicyErrors.None ||
                   errors == SslPolicyErrors.RemoteCertificateChainErrors;
            };
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.UserName, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}

