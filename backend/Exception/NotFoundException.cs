namespace backend.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, string id)
        : base($"The provided identifier: {id}, does not exist within {name}")
    { }
}
