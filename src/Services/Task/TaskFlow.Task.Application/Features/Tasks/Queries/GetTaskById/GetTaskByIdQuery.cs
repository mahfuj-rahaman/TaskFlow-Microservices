using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.DTOs;

namespace TaskFlow.Task.Application.Features.Tasks.Queries.GetTaskById;

/// <summary>
/// Query to get a task by ID
/// </summary>
public sealed record GetTaskByIdQuery(Guid TaskId) : IRequest<Result<TaskDto>>;
