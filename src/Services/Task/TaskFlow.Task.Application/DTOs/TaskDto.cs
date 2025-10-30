using TaskFlow.Task.Domain.Enums;

namespace TaskFlow.Task.Application.DTOs;

/// <summary>
/// Task data transfer object
/// </summary>
public sealed record TaskDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public TaskPriority Priority { get; init; }
    public Domain.Enums.TaskStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime? DueDate { get; init; }
    public bool IsOverdue { get; init; }
}
