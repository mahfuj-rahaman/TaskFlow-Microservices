using FluentValidation;

namespace TaskFlow.Task.Application.Features.Tasks.Commands.CreateTask;

/// <summary>
/// Validator for CreateTaskCommand
/// </summary>
public sealed class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.DueDate)
            .Must(dueDate => !dueDate.HasValue || dueDate.Value > DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("Due date must be in the future");
    }
}
