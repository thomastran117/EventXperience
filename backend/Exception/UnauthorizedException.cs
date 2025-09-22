namespace backend.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException()
        : base($"Invalid credientials are provided")
    { }
}
