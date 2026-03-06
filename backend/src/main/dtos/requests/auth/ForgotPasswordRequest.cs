using System.ComponentModel.DataAnnotations;

namespace backend.main.dtos.requests.auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public required string Email
        {
            get; set;
        }
    }
}
