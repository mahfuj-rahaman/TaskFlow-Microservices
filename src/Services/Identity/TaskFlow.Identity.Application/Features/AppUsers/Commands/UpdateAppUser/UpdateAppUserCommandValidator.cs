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

        // TODO: Add validation rules for UpdateAppUserCommand properties
    }
}
