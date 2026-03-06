using backend.main.exceptions.http;

namespace backend.main.errors.app
{
    public class InvalidOrExpiredRefreshTokenException : UnauthorizedException
    {
        private const string DefaultMessage = "Invalid or expired refresh token.";

        public InvalidOrExpiredRefreshTokenException()
            : base(DefaultMessage) { }

        public InvalidOrExpiredRefreshTokenException(string message)
            : base(message) { }

        public InvalidOrExpiredRefreshTokenException(string message, string details)
            : base(message, details) { }
    }
}
