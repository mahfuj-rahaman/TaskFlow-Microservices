using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Domain.Enums;

namespace TaskFlow.Task.Application.Features.Tasks.Commands.UpdateTask;

/// <summary>
/// Command to update an existing task
/// </summary>
public sealed record UpdateTaskCommand : IRequest<Result>
{
    public Guid TaskId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public TaskPriority? Priority { get; init; }
    public DateTime? DueDate { get; init; }
}
