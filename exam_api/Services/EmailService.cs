using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace exam_api.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration config;

    public EmailService(IConfiguration config)
    {
        this.config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        IConfigurationSection smtp_settings = config.GetSection("SmtpSettings");
        string? from_email = smtp_settings["FromEmail"];
        string? host = smtp_settings["Host"];
        int port = int.Parse(smtp_settings["Port"]);
        string? username = smtp_settings["Username"];
        string? password = smtp_settings["Password"];
        bool enableSsl = bool.Parse(smtp_settings["EnableSsl"]);
        
        MimeMessage email = new MimeMessage();
        email.From.Add(new MailboxAddress("Minipin", from_email));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        
        BodyBuilder builder = new BodyBuilder
        {
            HtmlBody = body
        };
        email.Body = builder.ToMessageBody();
        
        using SmtpClient smtp = new SmtpClient();
        await smtp.ConnectAsync(host, port, enableSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.None);
        await smtp.AuthenticateAsync(username, password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}