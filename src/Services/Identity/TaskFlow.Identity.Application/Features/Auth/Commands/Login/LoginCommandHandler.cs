using MediatR;
using TaskFlow.BuildingBlocks.Common.Results;
using TaskFlow.Identity.Application.DTOs;
using TaskFlow.Identity.Application.Interfaces;
using TaskFlow.Identity.Domain.Exceptions;

namespace TaskFlow.Identity.Application.Features.Auth.Commands.Login;

/// <summary>
/// Handler for LoginCommand
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IAppUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginCommandHandler(
        IAppUserRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Get user by email
        var user = await _repository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidCredentials", "Invalid email or password"));
        }

        // Check if account is locked
        if (user.IsLockedOut())
        {
            return Result.Failure<AuthResponse>(
                new Error("Auth.AccountLocked", $"Account is locked until {user.LockoutEndAt:yyyy-MM-dd HH:mm:ss} UTC"));
        }

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLoginAttempt();
            await _repository.UpdateAsync(user, cancellationToken);

            return Result.Failure<AuthResponse>(
                new Error("Auth.InvalidCredentials", "Invalid email or password"));
        }

        // Record successful login
        user.RecordSuccessfulLogin(request.IpAddress ?? "Unknown");
        await _repository.UpdateAsync(user, cancellationToken);

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
