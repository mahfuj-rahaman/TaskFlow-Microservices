using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.Identity.Domain.Events;

/// <summary>
/// Domain event raised when a AppUser is deleted
/// </summary>
public sealed record AppUserDeletedDomainEvent(Guid AppUserId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
