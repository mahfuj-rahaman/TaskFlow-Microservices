using Mapster;
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.DTOs;
using TaskFlow.Task.Application.Interfaces;

namespace TaskFlow.Task.Application.Features.Tasks.Queries.GetTaskById;

/// <summary>
/// Handler for GetTaskByIdQuery
/// </summary>
public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, Result<TaskDto>>
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskByIdQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);

            if (task == null)
            {
                return Result.Failure<TaskDto>(new Error(
                    "GetTaskById.NotFound",
                    $"Task with ID '{request.TaskId}' was not found"));
            }

            var taskDto = task.Adapt<TaskDto>();
            return Result.Success(taskDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<TaskDto>(new Error(
                "GetTaskById.Failed",
                $"Failed to retrieve task: {ex.Message}"));
        }
    }
}
