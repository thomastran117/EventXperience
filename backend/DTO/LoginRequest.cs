using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;
public class LoginRequest
{
    [Required]
    [StringLength(30, ErrorMessage = "Username cannot exceed 30 characters.")]
    public required string Username { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 30 characters.")]
    public required string Password { get; set; }
}