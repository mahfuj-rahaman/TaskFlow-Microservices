using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Identity.Application.DTOs;

namespace TaskFlow.Identity.Application.Features.Auth.Commands.Register;

/// <summary>
/// Command to register a new user
/// </summary>
public sealed record RegisterCommand : IRequest<Result<AuthResponse>>
{
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Password { get; init; }
    public required string ConfirmPassword { get; init; }
}
