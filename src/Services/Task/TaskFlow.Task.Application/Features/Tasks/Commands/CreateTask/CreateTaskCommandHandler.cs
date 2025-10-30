using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.Interfaces;
using TaskFlow.Task.Domain.Entities;

namespace TaskFlow.Task.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Handler for CreateTaskCommand
/// </summary>
public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<Guid>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create the task entity using factory method
            var task = TaskEntity.Create(
                request.Title,
                request.Description,
                request.UserId,
                request.Priority,
                request.DueDate);

            // Add to repository
            await _taskRepository.AddAsync(task, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Return success with task ID
            return Result.Success(task.Id);
        }
        catch (Exception ex)
        {
            // Return failure with error
            return Result.Failure<Guid>(new Error(
                "CreateTask.Failed",
                $"Failed to create task: {ex.Message}"));
        }
    }
}
