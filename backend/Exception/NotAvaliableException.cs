namespace backend.Exceptions
{
    public class NotAvaliableException : AppException
    {
        public NotAvaliableException(string message = "The service is not avaliable")
            : base(message, StatusCodes.Status503ServiceUnavailable) { }
    }
}