using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SignUpRequest
{
    [Required]
    [StringLength(30, ErrorMessage = "Username cannot exceed 30 characters.")]
    public required string Username { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 30 characters.")]
    public required string Password { get; set; }

    [Required]
    [RegularExpression("^(student|teacher|assistant)$", 
    ErrorMessage = "Usertype must be 'student', 'teacher' or 'assistant'.")]
    public required string Usertype { get; set; }
}
