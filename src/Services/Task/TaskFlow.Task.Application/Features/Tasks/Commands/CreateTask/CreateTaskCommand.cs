using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Domain.Enums;

namespace TaskFlow.Task.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Command to create a new task
/// </summary>
public sealed record CreateTaskCommand : IRequest<Result<Guid>>
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public TaskPriority Priority { get; init; } = TaskPriority.Medium;
    public DateTime? DueDate { get; init; }
}
