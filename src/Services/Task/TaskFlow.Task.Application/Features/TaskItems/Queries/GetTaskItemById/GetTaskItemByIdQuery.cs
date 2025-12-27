using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.DTOs;

namespace TaskFlow.Task.Application.Features.TaskItems.Queries.GetTaskItemById;

public sealed record GetTaskItemByIdQuery(Guid Id) : IRequest<Result<TaskItemDto>>;
