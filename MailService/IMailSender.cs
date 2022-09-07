using UserManagement.MailHelpers;

namespace UserManagement.MailService
{
    public interface IMailSender
    {
        
        Task SendEmailAsync(string toEmail, string subject, string content);
    }
}
