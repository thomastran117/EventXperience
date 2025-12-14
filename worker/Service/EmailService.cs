using System.IO;

using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;

using Polly;
using Polly.Retry;

using worker.Config;
using worker.Interfaces;
using worker.Utilities;

namespace worker.Services
{
    public sealed class EmailService : IEmailService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string? _username;
        private readonly string? _appPassword;

        private readonly AsyncRetryPolicy _smtpRetryPolicy;
        private bool IsConfigured { get; }

        public EmailService()
        {
            _username = EnvManager.Email;
            _appPassword = EnvManager.Password;

            if (string.IsNullOrWhiteSpace(_username) ||
                string.IsNullOrWhiteSpace(_appPassword))
            {
                Logger.Warn("[EmailService] EMAIL or EMAIL_PASSWORD not configured.");
                IsConfigured = false;
                return;
            }

            _smtpRetryPolicy = Policy
                .Handle<SmtpCommandException>()
                .Or<SmtpProtocolException>()
                .Or<IOException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, attempt)) +
                        TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500)),
                    onRetry: (ex, delay, attempt, _) =>
                    {
                        Logger.Warn(
                            $"[EmailService] SMTP retry {attempt}/3 in {delay.TotalSeconds:F1}s — {ex.Message}"
                        );
                    }
                );

            try
            {
                IsConfigured = EmailSmokeTest().GetAwaiter().GetResult();
                if (!IsConfigured)
                    Logger.Warn("[EmailService] SMTP smoke test failed.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[EmailService] Initialization failed");
                IsConfigured = false;
            }
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        public async Task SendVerificationEmailAsync(string toEmail, string token)
        {
            EnsureEnabled();

            var verifyUrl =
                $"http://localhost:3090/auth/verify?token={Uri.EscapeDataString(token)}";

            var message = BuildVerificationMessage(toEmail, verifyUrl);

            await SendAsync(message);
        }

        public Task SendResetPasswordEmailAsync(string toEmail, string token)
        {
            EnsureEnabled();
            throw new NotImplementedException();
        }

        public Task SendConfirmationEmailAsync(string toEmail, string token)
        {
            EnsureEnabled();
            throw new NotImplementedException();
        }

        public bool isEmailEnabled() => IsConfigured;

        // ------------------------------------------------------------------
        // Internal helpers
        // ------------------------------------------------------------------

        private async Task SendAsync(MimeMessage message)
        {
            await _smtpRetryPolicy.ExecuteAsync(async () =>
            {
                using var client = new SmtpClient();

                await client.ConnectAsync(
                    _smtpHost,
                    _smtpPort,
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(_username, _appPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            });
        }

        private async Task<bool> EmailSmokeTest()
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EventXperience System", _username));
            message.To.Add(MailboxAddress.Parse(_username!));
            message.Subject = "Email Service Smoke Test";
            message.Body = new TextPart("plain")
            {
                Text =
                    $"This is a test email to confirm SMTP configuration.\n\n" +
                    $"Timestamp: {DateTime.UtcNow:O}"
            };

            try
            {
                await SendAsync(message);
                Logger.Info("[EmailService] SMTP smoke test successful.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "[EmailService] SMTP smoke test failed");
                return false;
            }
        }

        private static MimeMessage BuildVerificationMessage(
            string toEmail,
            string verifyUrl
        )
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("EventXperience Team", EnvManager.Email));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = "Verify Your Email Address";

            var builder = new BodyBuilder
            {
                HtmlBody = $@"
                    <html>
                    <body style='font-family:Segoe UI,Roboto,Arial,sans-serif;background:#f7f8fc;'>
                    <div style='max-width:640px;margin:40px auto;background:#fff;border-radius:12px;padding:32px'>
                        <h2 style='color:#5a8dee'>Welcome to EventXperience 🎉</h2>
                        <p>Please confirm your email address by clicking the button below:</p>

                        <div style='text-align:center;margin:32px 0'>
                        <a href='{verifyUrl}'
                            style='background:#5a8dee;color:#fff;padding:14px 26px;
                                    border-radius:8px;text-decoration:none;font-weight:600'>
                            Verify My Email
                        </a>
                        </div>

                        <p style='font-size:14px;color:#666'>
                        This link expires in 10 minutes.
                        </p>
                    </div>
                    </body>
                </html>",
                TextBody =
                    $"Verify your email address using the link below:\n\n{verifyUrl}\n\n" +
                    $"This link expires in 10 minutes."
            };

            message.Body = builder.ToMessageBody();
            return message;
        }

        private void EnsureEnabled()
        {
            if (!IsConfigured)
            {
                Logger.Warn("[EmailService] Email service disabled.");
                throw new InvalidOperationException("Email service is not configured");
            }
        }
    }
}
