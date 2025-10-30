using TaskFlow.BuildingBlocks.Common.Exceptions;

namespace TaskFlow.Task.Domain.Exceptions;

/// <summary>
/// Base exception for Task domain violations
/// </summary>
public class TaskDomainException : DomainException
{
    public TaskDomainException(string message) : base(message)
    {
    }

    public TaskDomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when task title is invalid
/// </summary>
public class InvalidTaskTitleException : TaskDomainException
{
    public InvalidTaskTitleException(string title)
        : base($"Task title '{title}' is invalid. Title must be between 1 and 200 characters.")
    {
    }
}

/// <summary>
/// Exception thrown when task description is invalid
/// </summary>
public class InvalidTaskDescriptionException : TaskDomainException
{
    public InvalidTaskDescriptionException()
        : base("Task description is invalid. Description cannot exceed 2000 characters.")
    {
    }
}

/// <summary>
/// Exception thrown when trying to complete an already completed task
/// </summary>
public class TaskAlreadyCompletedException : TaskDomainException
{
    public TaskAlreadyCompletedException(Guid taskId)
        : base($"Task '{taskId}' is already completed.")
    {
    }
}

/// <summary>
/// Exception thrown when trying to perform action on cancelled task
/// </summary>
public class TaskCancelledException : TaskDomainException
{
    public TaskCancelledException(Guid taskId)
        : base($"Task '{taskId}' has been cancelled and cannot be modified.")
    {
    }
}
