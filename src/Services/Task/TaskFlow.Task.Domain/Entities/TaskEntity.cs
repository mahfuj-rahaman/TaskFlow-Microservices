using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.Task.Domain.Enums;
using TaskFlow.Task.Domain.Events;
using TaskFlow.Task.Domain.Exceptions;

namespace TaskFlow.Task.Domain.Entities;

/// <summary>
/// Task aggregate root
/// </summary>
public sealed class TaskEntity : AggregateRoot<Guid>
{
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 2000;

    // Private constructor for EF Core
    private TaskEntity(Guid id) : base(id)
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    private TaskEntity(
        Guid id,
        string title,
        string description,
        Guid userId,
        TaskPriority priority = TaskPriority.Medium) : base(id)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        Title = title;
        Description = description;
        UserId = userId;
        Priority = priority;
        Status = Enums.TaskStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskCreatedDomainEvent(id, title, userId));
    }

    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Task description
    /// </summary>
    public string Description { get; private set; }

    /// <summary>
    /// User ID who owns this task
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Task priority
    /// </summary>
    public TaskPriority Priority { get; private set; }

    /// <summary>
    /// Task status
    /// </summary>
    public Enums.TaskStatus Status { get; private set; }

    /// <summary>
    /// When the task was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the task was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// When the task was completed
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Task due date (optional)
    /// </summary>
    public DateTime? DueDate { get; private set; }

    /// <summary>
    /// Factory method to create a new task
    /// </summary>
    public static TaskEntity Create(
        string title,
        string description,
        Guid userId,
        TaskPriority priority = TaskPriority.Medium,
        DateTime? dueDate = null)
    {
        var task = new TaskEntity(Guid.NewGuid(), title, description, userId, priority);

        if (dueDate.HasValue)
        {
            task.SetDueDate(dueDate.Value);
        }

        return task;
    }

    /// <summary>
    /// Updates the task title
    /// </summary>
    public void UpdateTitle(string newTitle)
    {
        EnsureNotCancelled();
        ValidateTitle(newTitle);

        if (Title == newTitle)
            return;

        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the task description
    /// </summary>
    public void UpdateDescription(string newDescription)
    {
        EnsureNotCancelled();
        ValidateDescription(newDescription);

        if (Description == newDescription)
            return;

        Description = newDescription;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the task priority
    /// </summary>
    public void UpdatePriority(TaskPriority newPriority)
    {
        EnsureNotCancelled();

        if (Priority == newPriority)
            return;

        Priority = newPriority;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets or updates the due date
    /// </summary>
    public void SetDueDate(DateTime dueDate)
    {
        EnsureNotCancelled();

        if (dueDate < DateTime.UtcNow)
            throw new TaskDomainException("Due date cannot be in the past.");

        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Starts the task (moves to InProgress status)
    /// </summary>
    public void Start()
    {
        EnsureNotCancelled();

        if (Status == Enums.TaskStatus.Completed)
            throw new TaskAlreadyCompletedException(Id);

        if (Status == Enums.TaskStatus.InProgress)
            return;

        Status = Enums.TaskStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the task as completed
    /// </summary>
    public void Complete()
    {
        EnsureNotCancelled();

        if (Status == Enums.TaskStatus.Completed)
            throw new TaskAlreadyCompletedException(Id);

        Status = Enums.TaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TaskCompletedDomainEvent(Id, CompletedAt.Value));
    }

    /// <summary>
    /// Cancels the task
    /// </summary>
    public void Cancel()
    {
        if (Status == Enums.TaskStatus.Cancelled)
            return;

        if (Status == Enums.TaskStatus.Completed)
            throw new TaskDomainException("Cannot cancel a completed task.");

        Status = Enums.TaskStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the task is overdue
    /// </summary>
    public bool IsOverdue()
    {
        return DueDate.HasValue
            && Status != Enums.TaskStatus.Completed
            && Status != Enums.TaskStatus.Cancelled
            && DateTime.UtcNow > DueDate.Value;
    }

    private void EnsureNotCancelled()
    {
        if (Status == Enums.TaskStatus.Cancelled)
            throw new TaskCancelledException(Id);
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new InvalidTaskTitleException(title ?? string.Empty);

        if (title.Length > MaxTitleLength)
            throw new InvalidTaskTitleException(title);
    }

    private static void ValidateDescription(string description)
    {
        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            throw new InvalidTaskDescriptionException();
    }
}
