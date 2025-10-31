using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.CreateAppUser;

/// <summary>
/// Command to create a new AppUser
/// </summary>
public sealed record CreateAppUserCommand : IRequest<Result<Guid>>
{
    public required string Name { get; init; }
}
