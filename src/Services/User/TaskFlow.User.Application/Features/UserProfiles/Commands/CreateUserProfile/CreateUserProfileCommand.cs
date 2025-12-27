using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.User.Application.Features.UserProfiles.Commands.CreateUserProfile;

public sealed record CreateUserProfileCommand : IRequest<Result<Guid>>
{
    public required Guid AppUserId { get; init; }
    public required string DisplayName { get; init; }
}
