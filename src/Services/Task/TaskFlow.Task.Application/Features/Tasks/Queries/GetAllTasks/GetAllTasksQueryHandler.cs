using Mapster;
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.DTOs;
using TaskFlow.Task.Application.Interfaces;

namespace TaskFlow.Task.Application.Features.Tasks.Queries.GetAllTasks;

/// <summary>
/// Handler for GetAllTasksQuery
/// </summary>
public sealed class GetAllTasksQueryHandler : IRequestHandler<GetAllTasksQuery, Result<IEnumerable<TaskDto>>>
{
    private readonly ITaskRepository _taskRepository;

    public GetAllTasksQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<Result<IEnumerable<TaskDto>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tasks = request.UserId.HasValue
                ? await _taskRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken)
                : await _taskRepository.GetAllAsync(cancellationToken);

            var taskDtos = tasks.Adapt<IEnumerable<TaskDto>>();
            return Result.Success(taskDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<TaskDto>>(new Error(
                "GetAllTasks.Failed",
                $"Failed to retrieve tasks: {ex.Message}"));
        }
    }
}
