using TaskFlow.BuildingBlocks.Common.Domain;

namespace TaskFlow.Task.Domain.Events;

/// <summary>
/// Domain event raised when a task is created
/// </summary>
public sealed class TaskCreatedDomainEvent : IDomainEvent
{
    public TaskCreatedDomainEvent(Guid taskId, string title, Guid userId)
    {
        TaskId = taskId;
        Title = title;
        UserId = userId;
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }

    public Guid TaskId { get; }
    public string Title { get; }
    public Guid UserId { get; }
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
}
