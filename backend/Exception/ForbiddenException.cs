namespace backend.Exceptions
{
    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Unable to access or modify the requested resource")
            : base(message, StatusCodes.Status403Forbidden) { }
    }
}
