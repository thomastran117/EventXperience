using backend.main.exceptions.http;

namespace backend.main.errors.app
{
    public class InvalidCredentialsException : UnauthorizedException
    {
        private const string DefaultMessage = "Invalid email or password.";

        public InvalidCredentialsException()
            : base(DefaultMessage) { }

        public InvalidCredentialsException(string message)
            : base(message) { }

        public InvalidCredentialsException(string message, string details)
            : base(message, details) { }
    }
}
