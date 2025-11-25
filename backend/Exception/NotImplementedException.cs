namespace backend.Exceptions
{
    public class CustomNotImplementedException : AppException
    {
        public CustomNotImplementedException(string message = "The service is not implemented yet")
            : base(message, StatusCodes.Status501NotImplemented) { }
    }
}