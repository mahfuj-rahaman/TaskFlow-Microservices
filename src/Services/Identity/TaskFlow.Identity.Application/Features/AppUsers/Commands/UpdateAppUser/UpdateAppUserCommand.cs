using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.UpdateAppUser;

/// <summary>
/// Command to update an existing AppUser
/// </summary>
public sealed record UpdateAppUserCommand : IRequest<Result>
{
    public required Guid Id { get; init; }
    // TODO: Add properties to update for AppUser
}
