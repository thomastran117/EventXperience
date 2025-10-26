using System;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Threading.Tasks;
using backend.Config;

namespace backend.Test;
public class EmailService
{
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _username;
    private readonly string _appPassword;

    public EmailService()
    {
        _username = EnvManager.Email;
        _appPassword = EnvManager.Password;
    }

    public async Task SendTestEmailAsync()
    {
        // Replace with your Gmail and App Password
        var smtpHost = "smtp.gmail.com";
        var smtpPort = 587;
        var username = _username;
        var appPassword = _appPassword; // generated from Google Account → Security → App passwords
        var toEmail = "ttranm10134@gmail.com";

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(username));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = "✅ Gmail SMTP Test";
        message.Body = new TextPart("plain")
        {
            Text = "This is a test email sent using Gmail SMTP + App Password via C#."
        };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, appPassword);
            await client.SendAsync(message);
            Console.WriteLine("✅ Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send email: {ex.Message}");
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

}
