namespace Conversey.BL.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
