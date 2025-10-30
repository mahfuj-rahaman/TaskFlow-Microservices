using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Task.Application.Interfaces;

namespace TaskFlow.Task.Application.Features.Tasks.Commands.UpdateTask;

/// <summary>
/// Handler for UpdateTaskCommand
/// </summary>
public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, Result>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the task
            var task = await _taskRepository.GetByIdAsync(request.TaskId, cancellationToken);
            if (task == null)
            {
                return Result.Failure(new Error(
                    "UpdateTask.NotFound",
                    $"Task with ID '{request.TaskId}' was not found"));
            }

            // Update properties if provided
            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                task.UpdateTitle(request.Title);
            }

            if (request.Description != null)
            {
                task.UpdateDescription(request.Description);
            }

            if (request.Priority.HasValue)
            {
                task.UpdatePriority(request.Priority.Value);
            }

            if (request.DueDate.HasValue)
            {
                task.SetDueDate(request.DueDate.Value);
            }

            // Update in repository
            _taskRepository.Update(task);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(
                "UpdateTask.Failed",
                $"Failed to update task: {ex.Message}"));
        }
    }
}
