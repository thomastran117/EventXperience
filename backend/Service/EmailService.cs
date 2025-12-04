using backend.Config;
using backend.Exceptions;
using backend.Interfaces;
using backend.Utilities;

using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

namespace backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly string? _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string? _username;
        private readonly string? _appPassword;
        private bool IsConfigured { get; }

        public EmailService()
        {
            _username = EnvManager.Email;
            _appPassword = EnvManager.Password;

            if (string.IsNullOrWhiteSpace(_username) || string.IsNullOrWhiteSpace(_appPassword))
            {
                Logger.Warn("EmailService: EMAIL or EMAIL_PASSWORD is not configured. Email features will be disabled.");
                IsConfigured = false;
                return;
            }

            try
            {
                IsConfigured = EmailSmokeTest().GetAwaiter().GetResult();

                if (!IsConfigured)
                    Logger.Warn("EmailService failed — SMTP test unsuccessful.");
            }
            catch (Exception ex)
            {
                Logger.Error($"EmailService initialization error: {ex.Message}");
                IsConfigured = false;
            }

            if (!IsConfigured)
            {
                Logger.Warn("EmailService: EMAIL or EMAIL_PASSWORD is not configured. Email features will be disabled.");
            }
        }

        private async Task<bool> EmailSmokeTest()
        {
            var testRecipient = _username;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EventXperience System", _username));
            message.To.Add(MailboxAddress.Parse(testRecipient));
            message.Subject = "Email Service Smoke Test";
            message.Body = new TextPart("plain")
            {
                Text = $"This is a test email from EventXperience to confirm your email configuration works.\n\n" +
                    $"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                    $"If you received this email, your SMTP setup is functional.\n\n" +
                    $"— EventXperience System"
            };

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_username, _appPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Email service failed: {ex.Message}");
                return false;
            }
        }

        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            if (!isEmailEnabled())
            {
                string warning = "Email service is misconfigured and unavaliable";
                Logger.Warn(warning);
                throw new NotAvaliableException("warning");
            }

            var verifyUrl = $"http://localhost:3090/auth/verify?token={Uri.EscapeDataString(token)}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EventXperience Team", _username));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Verify Your Email Address";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                <html>
                <body style='margin:0;padding:0;background:#f7f8fc;font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#333;'>
                    <div style='max-width:640px;margin:40px auto;background:#ffffff;border-radius:16px;box-shadow:0 4px 12px rgba(0,0,0,0.07);overflow:hidden;'>

                    <!-- Header -->
                    <div style='
                        background:linear-gradient(135deg, #5a8dee, #4fc3f7);
                        padding:28px 20px;
                        text-align:center;
                        color:#ffffff;
                        font-size:24px;
                        font-weight:700;
                        letter-spacing:0.4px;'>
                        Welcome to EventXperience 🎉
                    </div>

                    <!-- Body -->
                    <div style='padding:40px 36px;line-height:1.7;'>
                        <p style='font-size:17px;margin-top:0;margin-bottom:20px;'>Hey there,</p>
                        <p style='font-size:16px;margin:0 0 18px;'>
                        We’re thrilled to have you join our community of event-goers and organizers!  
                        Before you dive into discovering exciting experiences, please take a quick moment to confirm your email address.
                        </p>

                        <div style='text-align:center;margin:38px 0;'>
                        <a href='{verifyUrl}'
                            style='background:#5a8dee;
                                    color:#ffffff;
                                    text-decoration:none;
                                    padding:14px 30px;
                                    border-radius:8px;
                                    font-size:16px;
                                    font-weight:600;
                                    display:inline-block;
                                    box-shadow:0 3px 8px rgba(90,141,238,0.3);'>
                            Verify My Email
                        </a>
                        </div>

                        <p style='font-size:14px;color:#555;margin-top:18px;'>
                        If you didn’t create an account with <strong>EventXperience</strong>, you can safely ignore this message — no action is needed.
                        </p>

                        <p style='font-size:14px;color:#555;margin-top:30px;'>
                        This link will expire in <strong>10 minutes</strong> to help keep your account secure.
                        </p>

                        <div style='margin-top:35px;text-align:center;'>
                        <p style='font-size:15px;color:#444;margin-bottom:8px;'>Can’t wait to see what you’ll discover next!</p>
                        <p style='font-size:15px;color:#5a8dee;font-weight:600;'>— The EventXperience Team 💙</p>
                        </div>
                    </div>

                    <!-- Footer -->
                    <div style='background:#f3f5f7;padding:16px;text-align:center;font-size:12px;color:#888;'>
                        © {DateTime.UtcNow.Year} EventXperience. All rights reserved.<br />
                        Crafted with for event enthusiasts everywhere.
                    </div>
                    </div>
                </body>
                </html>",

                TextBody =
                    $"Welcome to EventXperience!\n\n" +
                    $"We’re excited to have you join us. Please verify your email address using the link below:\n\n{verifyUrl}\n\n" +
                    $"This link will expire in 10 minutes for your security.\n\n" +
                    $"— The EventXperience Team"
            };

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_username, _appPassword);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to send verification email: {ex.Message}");
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        public async Task SendResetPasswordEmailAsync(string toEmail, string token)
        {
            if (!isEmailEnabled())
            {
                string warning = "Email service is misconfigured and unavaliable";
                Logger.Warn(warning);
                throw new NotAvaliableException("warning");
            }
            throw new NotImplementedException("Not implemented yet");
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string token)
        {
            if (!isEmailEnabled())
            {
                string warning = "Email service is misconfigured and unavaliable";
                Logger.Warn(warning);
                throw new NotAvaliableException("warning");
            }
            throw new NotImplementedException("Not implemented yet");
        }

        public bool isEmailEnabled()
        {
            return IsConfigured;
        }
    }
}
