using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.User.Application.Interfaces;

namespace TaskFlow.User.Application.Features.UserProfiles.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
{
    private readonly IUserProfileRepository _repository;

    public UpdateUserProfileCommandHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure(new Error("UserProfile.NotFound", "Profile not found"));
        }

        entity.UpdateProfile(request.DisplayName, request.Bio, request.PhoneNumber);
        await _repository.UpdateAsync(entity, cancellationToken);
        return Result.Success();
    }
}
