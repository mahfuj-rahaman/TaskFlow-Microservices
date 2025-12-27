using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.User.Domain.Events;

/// <summary>
/// Domain event raised when a UserProfile is created
/// </summary>
public sealed record UserProfileCreatedDomainEvent(Guid UserProfileId, Guid AppUserId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
