using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.Identity.Domain.Events;

/// <summary>
/// Domain event raised when a AppUser is created
/// </summary>
public sealed record AppUserCreatedDomainEvent(Guid AppUserId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
