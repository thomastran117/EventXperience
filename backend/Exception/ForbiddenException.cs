namespace backend.Exceptions;
public class ForbiddenException : Exception
{
public ForbiddenException(string name, string id)
    : base($"You are not alloewd to edit the {name} with id of {id}")
{ }
}
