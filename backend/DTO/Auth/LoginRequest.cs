namespace backend.DTOs
{
    public class LoginRequest : AuthRequest
    {
        public new bool RememberMe { get; set; } = false;
    }
}