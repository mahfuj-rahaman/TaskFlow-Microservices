using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.User.Application.Features.UserProfiles.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand : IRequest<Result>
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? PhoneNumber { get; init; }
}
