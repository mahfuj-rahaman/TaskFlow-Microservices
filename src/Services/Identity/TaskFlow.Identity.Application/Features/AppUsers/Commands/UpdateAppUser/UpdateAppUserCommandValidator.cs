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
            .NotEmpty()
            .WithMessage("Id is required");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");
    }
}
