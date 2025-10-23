namespace backend.DTOs
{
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
}
