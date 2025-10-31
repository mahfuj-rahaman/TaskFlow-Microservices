namespace TaskFlow.Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when a domain rule is violated in AppUser
/// </summary>
public sealed class AppUserDomainException : Exception
{
    public AppUserDomainException(string message) : base(message)
    {
    }

    public AppUserDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
