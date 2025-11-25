namespace backend.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message = "Bad request")
            : base(message, StatusCodes.Status400BadRequest) { }
    }
}