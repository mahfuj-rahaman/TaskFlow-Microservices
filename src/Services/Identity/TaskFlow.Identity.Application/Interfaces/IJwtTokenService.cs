using TaskFlow.Identity.Domain.Entities;

namespace TaskFlow.Identity.Application.Interfaces;

/// <summary>
/// Interface for JWT token operations
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for a user
    /// </summary>
    string GenerateAccessToken(AppUserEntity user);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a token and returns the user ID
    /// </summary>
    Guid? ValidateToken(string token);
}
