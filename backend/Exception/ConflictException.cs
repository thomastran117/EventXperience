namespace backend.Exceptions;
public class ConflictException : Exception
{
    public ConflictException(string name, string id)
        : base($"The provided identifier: {id}, already exist within {name}")
    { }
}
