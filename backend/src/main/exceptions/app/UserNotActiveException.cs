using backend.main.exceptions.http;

namespace backend.main.errors.app
{
    public class UserNotActiveException : ForbiddenException
    {
        private const string DefaultMessage = "Account is not active.";

        public UserNotActiveException()
            : base(DefaultMessage) { }

        public UserNotActiveException(string message)
            : base(message) { }

        public UserNotActiveException(string message, string details)
            : base(message, details) { }
    }
}
