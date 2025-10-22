namespace backend.Exceptions
{
    public class NotAvaliableException : AppException
    {
        public NotAvaliableException(string message)
            : base(message, StatusCodes.Status503ServiceUnavailable) { }
    }
}