using Mapster;
using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.User.Application.DTOs;
using TaskFlow.User.Application.Interfaces;

namespace TaskFlow.User.Application.Features.UserProfiles.Queries.GetUserProfileById;

public sealed class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, Result<UserProfileDto>>
{
    private readonly IUserProfileRepository _repository;

    public GetUserProfileByIdQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<UserProfileDto>(new Error("UserProfile.NotFound", "Profile not found"));
        }

        return Result.Success(entity.Adapt<UserProfileDto>());
    }
}
