namespace TaskFlow.Task.Domain.Exceptions;

public sealed class TaskItemNotFoundException : Exception
{
    public TaskItemNotFoundException(Guid id)
        : base($"TaskItem with ID '{id}' was not found")
    {
    }
}
