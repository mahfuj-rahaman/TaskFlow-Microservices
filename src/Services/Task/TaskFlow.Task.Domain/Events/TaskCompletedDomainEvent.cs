using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.Task.Domain.Events;

/// <summary>
/// Domain event raised when a task is completed
/// </summary>
public sealed class TaskCompletedDomainEvent : IDomainEvent
{
    public TaskCompletedDomainEvent(Guid taskId, DateTime completedAt)
    {
        TaskId = taskId;
        CompletedAt = completedAt;
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid TaskId { get; }
    public DateTime CompletedAt { get; }
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
}
