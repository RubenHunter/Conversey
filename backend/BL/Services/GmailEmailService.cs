using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Conversey.BL.Services;

public class GmailEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailEmailService> _logger;

    public GmailEmailService(IConfiguration configuration, ILogger<GmailEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "resend";
        var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@conversey.be";
        var senderPassword = _configuration["EmailSettings:SenderPassword"] ?? string.Empty;
        var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        
        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(smtpUsername, senderPassword)
        };

        using var message = new MailMessage(senderEmail, toEmail, subject, body)
        {
            IsBodyHtml = false
        };

        await client.SendMailAsync(message);
        _logger.LogInformation("Email sent to {ToEmail} with subject '{Subject}'", toEmail, subject);
    }
}
