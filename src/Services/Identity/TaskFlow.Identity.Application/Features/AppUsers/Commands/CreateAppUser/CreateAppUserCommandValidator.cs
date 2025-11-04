using FluentValidation;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.CreateAppUser;

/// <summary>
/// Validator for CreateAppUserCommand
/// </summary>
public sealed class CreateAppUserCommandValidator : AbstractValidator<CreateAppUserCommand>
{
    public CreateAppUserCommandValidator()
    {
        // TODO: Add validation rules for CreateAppUserCommand properties
    }
}
