using backend.main.models.core;

namespace backend.main.models.other
{
    public class Token
    {
        public string AccessToken
        {
            get; set;
        }
        public string RefreshToken
        {
            get; set;
        }
        public TimeSpan RefreshTokenLifetime
        {
            get; set;
        }

        public Token(string accessToken, string refreshToken, TimeSpan refreshTokenLifetime)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            RefreshTokenLifetime = refreshTokenLifetime;
        }
    }

    public class RefreshTokenIssue
    {
        public string Value
        {
            get; set;
        }
        public TimeSpan Lifetime
        {
            get; set;
        }

        public RefreshTokenIssue(string value, TimeSpan lifetime)
        {
            Value = value;
            Lifetime = lifetime;
        }
    }

    public class UserToken
    {
        public Token token
        {
            get; set;
        }
        public User user
        {
            get; set;
        }
        public UserToken(Token token, User user)
        {
            this.token = token;
            this.user = user;
        }
    }
}
