using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Task.Application.Features.TaskItems.Commands.UpdateTaskItem;

public sealed record UpdateTaskItemCommand : IRequest<Result>
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public int Priority { get; init; } = 2;
}
