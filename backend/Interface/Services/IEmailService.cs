using backend.Models;

namespace backend.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string toEmail, string token);
        Task SendResetPasswordEmailAsync(string toEmail, string token);
        Task SendConfirmationEmailAsync(string toEmail, string token);
        bool isEmailEnabled();
    }
}