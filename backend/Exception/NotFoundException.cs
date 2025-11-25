namespace backend.Exceptions
{
    public class NotFoundException : AppException
    {
        public NotFoundException(string message = "The requested resouce is not found")
            : base(message, StatusCodes.Status404NotFound) { }
    }
}
