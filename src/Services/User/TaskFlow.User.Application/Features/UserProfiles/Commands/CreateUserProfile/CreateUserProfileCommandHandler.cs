using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.User.Application.Interfaces;
using TaskFlow.User.Domain.Entities;

namespace TaskFlow.User.Application.Features.UserProfiles.Commands.CreateUserProfile;

public sealed class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, Result<Guid>>
{
    private readonly IUserProfileRepository _repository;

    public CreateUserProfileCommandHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsByAppUserIdAsync(request.AppUserId, cancellationToken))
        {
            return Result.Failure<Guid>(new Error("UserProfile.AlreadyExists", "Profile already exists for this user"));
        }

        var entity = UserProfileEntity.Create(request.AppUserId, request.DisplayName);
        await _repository.AddAsync(entity, cancellationToken);
        return Result.Success(entity.Id);
    }
}
