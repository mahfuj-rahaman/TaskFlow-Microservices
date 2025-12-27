using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Identity.Application.DTOs;

namespace TaskFlow.Identity.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to login a user
/// </summary>
public sealed record LoginCommand : IRequest<Result<AuthResponse>>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? IpAddress { get; init; }
}
