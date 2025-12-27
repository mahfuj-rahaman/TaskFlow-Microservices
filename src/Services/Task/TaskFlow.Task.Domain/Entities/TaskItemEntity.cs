using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.Task.Domain.Events;

namespace TaskFlow.Task.Domain.Entities;

public sealed class TaskItemEntity : AggregateRoot<Guid>
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Enums.TaskStatus Status { get; private set; } = Enums.TaskStatus.Todo;
    public Enums.TaskPriority Priority { get; private set; } = Enums.TaskPriority.Medium;
    public Guid CreatedByUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public List<string> Tags { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private TaskItemEntity(Guid id) : base(id)
    {
    }

    public static TaskItemEntity Create(string title, string? description, Guid createdByUserId, Enums.TaskPriority priority = Enums.TaskPriority.Medium)
    {
        var entity = new TaskItemEntity(Guid.NewGuid())
        {
            Title = title,
            Description = description,
            CreatedByUserId = createdByUserId,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            Status = Enums.TaskStatus.Todo,
            Tags = new List<string>()
        };

        entity.RaiseDomainEvent(new TaskItemCreatedDomainEvent(entity.Id));
        return entity;
    }

    public void Update(string title, string? description, Enums.TaskPriority priority)
    {
        Title = title;
        Description = description;
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignTo(Guid userId)
    {
        AssignedToUserId = userId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDueDate(DateTime dueDate)
    {
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeStatus(Enums.TaskStatus newStatus)
    {
        Status = newStatus;
        if (newStatus == Enums.TaskStatus.Done)
        {
            CompletedAt = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddTag(string tag)
    {
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveTag(string tag)
    {
        Tags.Remove(tag);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        RaiseDomainEvent(new TaskItemDeletedDomainEvent(Id));
    }
}
