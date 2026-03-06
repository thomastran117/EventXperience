namespace backend.main.dtos.requests.auth
{
    public class LoginRequest : AuthRequest
    {
        public new bool RememberMe { get; set; } = false;
    }
}
