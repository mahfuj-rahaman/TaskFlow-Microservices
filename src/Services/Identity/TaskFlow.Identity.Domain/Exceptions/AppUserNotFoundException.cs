namespace TaskFlow.Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when an AppUser is not found
/// </summary>
public sealed class AppUserNotFoundException : Exception
{
    public AppUserNotFoundException(Guid id)
        : base($"AppUser with ID '{id}' was not found")
    {
    }

    public AppUserNotFoundException(string email)
        : base($"AppUser with email '{email}' was not found")
    {
    }
}
