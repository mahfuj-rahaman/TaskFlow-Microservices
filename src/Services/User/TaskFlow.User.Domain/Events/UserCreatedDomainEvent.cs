using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.User.Domain.Enums;

namespace TaskFlow.User.Domain.Events;

/// <summary>
/// Domain event raised when a user is created
/// </summary>
public sealed record UserCreatedDomainEvent(
    Guid UserId, string Email, string FirstName, string LastName, DateTime DateOfBirth, UserStatus Status) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;

    public Guid EventId => Guid.NewGuid();
}
