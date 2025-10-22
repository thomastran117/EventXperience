namespace backend.Exceptions
{
    public class NotImplementedException : AppException
    {
        public NotImplementedException(string message)
            : base(message, StatusCodes.Status501NotImplemented) { }
    }
}