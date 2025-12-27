using Mapster;
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.DTOs;
using TaskFlow.Task.Application.Interfaces;

namespace TaskFlow.Task.Application.Features.TaskItems.Queries.GetTaskItemById;

public sealed class GetTaskItemByIdQueryHandler : IRequestHandler<GetTaskItemByIdQuery, Result<TaskItemDto>>
{
    private readonly ITaskItemRepository _repository;

    public GetTaskItemByIdQueryHandler(ITaskItemRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<Result<TaskItemDto>> Handle(GetTaskItemByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<TaskItemDto>(new Error("TaskItem.NotFound", "Task not found"));
        }

        return Result.Success(entity.Adapt<TaskItemDto>());
    }
}
