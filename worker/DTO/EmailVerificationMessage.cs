namespace worker.DTOs;

public sealed class EmailVerificationMessage
{
    public required string Email { get; init; }
    public required string Token { get; init; }
}
