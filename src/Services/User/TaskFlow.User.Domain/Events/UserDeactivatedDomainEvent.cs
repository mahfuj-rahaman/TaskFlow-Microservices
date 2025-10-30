using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Events;

/// <summary>
/// Domain event raised when a user is deactivated
/// </summary>
public sealed record UserDeactivatedDomainEvent(Guid UserId) : IDomainEvent
{
    public DateTime OccurredOn => DateTime.UtcNow;

    public Guid EventId => Guid.NewGuid();
}
