using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Events;

/// <summary>
/// Domain event raised when a UserProfile is deleted
/// </summary>
public sealed record UserProfileDeletedDomainEvent(Guid UserProfileId, Guid AppUserId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
