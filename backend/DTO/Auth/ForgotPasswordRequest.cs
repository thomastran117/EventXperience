using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
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
