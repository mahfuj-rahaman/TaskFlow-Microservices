using FluentValidation;

namespace TaskFlow.Task.Application.Features.TaskItems.Commands.CreateTaskItem;

public sealed class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
{
    public CreateTaskItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CreatedByUserId).NotEmpty();
        RuleFor(x => x.Priority).InclusiveBetween(1, 4);
    }
}
