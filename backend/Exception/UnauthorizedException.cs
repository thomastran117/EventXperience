namespace backend.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized")
            : base(message, StatusCodes.Status401Unauthorized) { }
    }
}