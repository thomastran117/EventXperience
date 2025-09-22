using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class AuthResponse
{
    public AuthResponse(int id, string email, string userType, string token)
    {
        Id = id;
        Email = email;
        Usertype = userType;
        Token = token;
    }

    public int Id { get; set; }
    public string Email { get; set; }
    public string Usertype { get; set; }
    public string Token { get; set; }
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 30 characters.")]
    public required string Password { get; set; }
}

public class SignUpRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(30, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 30 characters.")]
    public required string Password { get; set; }

    [Required]
    [RegularExpression("^(participant|organizer|volunteer)$",
    ErrorMessage = "Usertype must be 'participant', 'organizer' or 'volunteer'.")]
    public required string Usertype { get; set; }
}
