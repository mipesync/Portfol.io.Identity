using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace Portfol.io.Identity.Common.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new MimeMessage();

            var username = _config.GetSection("HostUsername").Value;

            emailMessage.From.Add(new MailboxAddress("Администрация Porfol.io", username));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            using (var client = new SmtpClient())
            {
                string host = _config.GetSection("Host").Value,
                    password = _config.GetSection("Password").Value;
                int port = Convert.ToInt32(_config.GetSection("Port").Value);

                await client.ConnectAsync(host, port, false);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }
        }
    }
}
