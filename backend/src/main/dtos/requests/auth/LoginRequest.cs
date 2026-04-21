using System.ComponentModel.DataAnnotations;

namespace backend.main.dtos.requests.auth
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public required string Email
        {
            get; set;
        }

        [Required]
        public required string Password
        {
            get; set;
        }

        public bool RememberMe { get; set; } = false;

        [Required]
        public required string Captcha
        {
            get; set;
        }
    }
}
