using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public abstract class OAuthRequest
    {
        [Required]
        public required string Token
        {
            get; set;
        }
    }
}
