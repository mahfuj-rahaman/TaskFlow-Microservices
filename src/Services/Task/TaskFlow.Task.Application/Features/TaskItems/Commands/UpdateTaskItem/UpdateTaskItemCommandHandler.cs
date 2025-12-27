using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.Interfaces;
using TaskFlow.Task.Domain.Enums;

namespace TaskFlow.Task.Application.Features.TaskItems.Commands.UpdateTaskItem;

public sealed class UpdateTaskItemCommandHandler : IRequestHandler<UpdateTaskItemCommand, Result>
{
    private readonly ITaskItemRepository _repository;

    public UpdateTaskItemCommandHandler(ITaskItemRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<Result> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure(new Error("TaskItem.NotFound", "Task not found"));
        }

        entity.Update(request.Title, request.Description, (TaskPriority)request.Priority);
        await _repository.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
