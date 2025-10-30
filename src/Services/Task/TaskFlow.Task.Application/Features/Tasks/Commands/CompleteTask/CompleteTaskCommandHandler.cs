using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.Interfaces;

namespace TaskFlow.Task.Application.Features.Tasks.Commands.CompleteTask;

/// <summary>
/// Handler for CompleteTaskCommand
/// </summary>
public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, Result>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the task
            var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
            if (task == null)
            {
                return Result.Failure(new Error(
                    "CompleteTask.NotFound",
                    $"Task with ID '{request.TaskId}' was not found"));
            }

            // Complete the task
            task.Complete();

            // Update in repository
            _taskRepository.Update(task);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(
                "CompleteTask.Failed",
                $"Failed to complete task: {ex.Message}"));
        }
    }
}
