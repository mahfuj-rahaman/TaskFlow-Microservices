namespace TaskFlow.Task.Domain.Enums;

/// <summary>
/// Task status enumeration
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// Task is pending and not yet started
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Task is currently in progress
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Task has been completed
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Task has been cancelled
    /// </summary>
    Cancelled = 3
}
