namespace TaskFlow.Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when trying to create an AppUser that already exists
/// </summary>
public sealed class AppUserAlreadyExistsException : Exception
{
    public AppUserAlreadyExistsException(string email)
        : base($"AppUser with email '{email}' already exists")
    {
    }

    public AppUserAlreadyExistsException(string field, string value)
        : base($"AppUser with {field} '{value}' already exists")
    {
    }
}
