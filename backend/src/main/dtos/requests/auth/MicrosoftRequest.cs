namespace backend.main.dtos.requests.auth
{
    public class MicrosoftRequest : OAuthRequest
    {
        public string? Nonce { get; set; }
    }
}
