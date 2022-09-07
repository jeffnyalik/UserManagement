using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using UserManagement.MailHelpers;
using UserManagement.MailSettings;

namespace UserManagement.MailService
{
    public class MailSender : IMailSender
    {
        private readonly EmailConfiguration _emailConfiguration;
        public MailSender(IOptions<EmailConfiguration> emailConfiguration)
        {
            _emailConfiguration = emailConfiguration.Value;
        }

      
        public async Task SendEmailAsync(string toEmail, string subject, string content)
        {
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_emailConfiguration.From);
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            var builder = new BodyBuilder();

            builder.HtmlBody = content;
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            smtp.Connect(_emailConfiguration.Host, _emailConfiguration.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_emailConfiguration.UserName, _emailConfiguration.Password);

            /*smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("bizname1990@gmail.com", "ommafmxfowobugft");*/
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

        }
    }
}
