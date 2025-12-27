using FluentValidation;

namespace TaskFlow.User.Application.Features.UserProfiles.Commands.CreateUserProfile;

public sealed class CreateUserProfileCommandValidator : AbstractValidator<CreateUserProfileCommand>
{
    public CreateUserProfileCommandValidator()
    {
        RuleFor(x => x.AppUserId).NotEmpty();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(100);
    }
}
