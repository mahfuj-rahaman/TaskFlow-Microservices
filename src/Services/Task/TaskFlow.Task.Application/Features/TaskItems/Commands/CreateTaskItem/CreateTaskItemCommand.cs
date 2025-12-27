using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Task.Application.Features.TaskItems.Commands.CreateTaskItem;

public sealed record CreateTaskItemCommand : IRequest<Result<Guid>>
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public int Priority { get; init; } = 2;
}
