using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;

namespace TaskFlow.Identity.Application.Features.AppUsers.Commands.CreateAppUser;

/// <summary>
/// Command to create a new AppUser
/// </summary>
public sealed record CreateAppUserCommand : IRequest<Result<Guid>>
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Password { get; init; }
}
