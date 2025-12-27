using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Application.Interfaces;
using TaskFlow.Identity.Domain.Entities;
using TaskFlow.Identity.Domain.Exceptions;

namespace TaskFlow.Identity.Application.Features.Auth.Commands.Register;

/// <summary>
/// Handler for RegisterCommand
/// </summary>
public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthResponse>>
{
    private readonly IAppUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterCommandHandler(
        IAppUserRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _repository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result.Failure<AuthResponse>(
                new Error("Auth.EmailExists", $"Email '{request.Email}' is already registered"));
        }

        // Check if username already exists
        if (await _repository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            return Result.Failure<AuthResponse>(
                new Error("Auth.UsernameExists", $"Username '{request.Username}' is already taken"));
        }

        // Hash the password
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Create the user
        var user = AppUserEntity.Create(
            request.Username,
            request.Email,
            request.FirstName,
            request.LastName,
            passwordHash);

        await _repository.AddAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var response = new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.Roles,
            Token = new AuthTokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            }
        };

        return Result.Success(response);
    }
}
