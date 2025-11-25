namespace backend.Exceptions
{
    public class ConflictException : AppException
    {
        public ConflictException(string message = "The requested action causes a conflcit with an existing resource")
            : base(message, StatusCodes.Status409Conflict) { }
    }
}