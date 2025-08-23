namespace backend.Exceptions;
public class BadRequestException : Exception
{
    public BadRequestException(string name, string id)
        : base($"Invalid inputs. Please correct them.")
    { }
}