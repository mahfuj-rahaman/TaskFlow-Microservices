using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.DTOs;

namespace TaskFlow.Task.Application.Features.Tasks.Queries.GetAllTasks;

/// <summary>
/// Query to get all tasks
/// </summary>
public sealed record GetAllTasksQuery(Guid? UserId = null) : IRequest<Result<IEnumerable<TaskDto>>>;
