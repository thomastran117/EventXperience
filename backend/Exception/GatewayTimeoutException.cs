namespace backend.Exceptions
{
    public class GatewayTimeoutException : AppException
    {
        private const string DefaultMessage = "Gateway timeout";
        private const int code = StatusCodes.Status502BadGateway;

        public GatewayTimeoutException()
            : base(DefaultMessage, code) { }

        public GatewayTimeoutException(string message)
            : base(message, code) { }

        public GatewayTimeoutException(string message, string details)
            : base(message, code, details) { }
    }
}
