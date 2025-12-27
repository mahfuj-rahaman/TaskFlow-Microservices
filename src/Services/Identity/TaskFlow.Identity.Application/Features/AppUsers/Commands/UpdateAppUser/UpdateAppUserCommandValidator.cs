using FluentValidation;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.UpdateAppUser;

/// <summary>
/// Validator for UpdateAppUserCommand
/// </summary>
public sealed class UpdateAppUserCommandValidator : AbstractValidator<UpdateAppUserCommand>
{
    public UpdateAppUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");
    }
}
