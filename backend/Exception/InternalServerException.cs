namespace backend.Exceptions
{
    public class InternalServerException : AppException
    {
        public InternalServerException(string message = "Internal server error")
            : base(message, StatusCodes.Status500InternalServerError) { }
    }
}
