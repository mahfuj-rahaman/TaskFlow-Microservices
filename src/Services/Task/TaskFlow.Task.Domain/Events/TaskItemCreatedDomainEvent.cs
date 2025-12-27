using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.Task.Domain.Events;

public sealed record TaskItemCreatedDomainEvent(Guid TaskItemId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
