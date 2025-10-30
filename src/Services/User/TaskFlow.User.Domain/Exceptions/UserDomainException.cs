using TaskFlow.BuildingBlocks.Common.Exceptions;

namespace TaskFlow.User.Domain.Exceptions;

/// <summary>
/// Exception for user domain rule violations
/// </summary>
public sealed class UserDomainException : DomainException
{
    public UserDomainException(string message) : base(message)
    {
    }

    public UserDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
