namespace TaskFlow.User.Domain.Exceptions;

/// <summary>
/// Exception thrown when a UserProfile is not found
/// </summary>
public sealed class UserProfileNotFoundException : Exception
{
    public UserProfileNotFoundException(Guid id)
        : base($"UserProfile with ID '{id}' was not found")
    {
    }

    public UserProfileNotFoundException(Guid appUserId, bool byAppUserId)
        : base($"UserProfile for AppUser ID '{appUserId}' was not found")
    {
    }
}
