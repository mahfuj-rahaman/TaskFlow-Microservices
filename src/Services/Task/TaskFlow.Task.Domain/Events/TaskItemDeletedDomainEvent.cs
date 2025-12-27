using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.Task.Domain.Events;

public sealed record TaskItemDeletedDomainEvent(Guid TaskItemId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
