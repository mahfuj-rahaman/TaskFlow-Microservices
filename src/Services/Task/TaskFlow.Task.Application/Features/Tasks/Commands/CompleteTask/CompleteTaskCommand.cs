using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Task.Application.Features.Tasks.Commands.CompleteTask;

/// <summary>
/// Command to complete a task
/// </summary>
public sealed record CompleteTaskCommand(Guid TaskId) : IRequest<Result>;
