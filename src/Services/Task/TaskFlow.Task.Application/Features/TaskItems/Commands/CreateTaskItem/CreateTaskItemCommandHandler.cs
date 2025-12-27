using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.Interfaces;
using TaskFlow.Task.Domain.Entities;
using TaskFlow.Task.Domain.Enums;

namespace TaskFlow.Task.Application.Features.TaskItems.Commands.CreateTaskItem;

public sealed class CreateTaskItemCommandHandler : IRequestHandler<CreateTaskItemCommand, Result<Guid>>
{
    private readonly ITaskItemRepository _repository;

    public CreateTaskItemCommandHandler(ITaskItemRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<Result<Guid>> Handle(CreateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var entity = TaskItemEntity.Create(
            request.Title,
            request.Description,
            request.CreatedByUserId,
            (TaskPriority)request.Priority);

        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
