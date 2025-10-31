using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Events;

public sealed record AppUserCreatedDomainEvent(
    Guid AppUserId,
    string Username,
    string Email,
    Guid UserEntityId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
