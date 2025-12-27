namespace TaskFlow.User.Domain.Exceptions;

/// <summary>
/// Exception thrown when trying to create a UserProfile that already exists
/// </summary>
public sealed class UserProfileAlreadyExistsException : Exception
{
    public UserProfileAlreadyExistsException(Guid appUserId)
        : base($"UserProfile for AppUser ID '{appUserId}' already exists")
    {
    }
}
