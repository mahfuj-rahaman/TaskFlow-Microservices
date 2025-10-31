using FluentValidation;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.CreateAppUser;

/// <summary>
/// Validator for CreateAppUserCommand
/// </summary>
public sealed class CreateAppUserCommandValidator : AbstractValidator<CreateAppUserCommand>
{
    public CreateAppUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");
    }
}
