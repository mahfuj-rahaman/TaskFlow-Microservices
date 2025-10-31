using TaskFlow.User.Domain.Exceptions;

namespace TaskFlow.User.Domain.ValueObjects;

/// <summary>
/// Refresh token value object
/// </summary>
public sealed class RefreshToken
{
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string CreatedByIp { get; private set; } = string.Empty;
    public DateTime? RevokedAt { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? ReplacedByToken { get; private set; }

    /// <summary>
    /// Checks if token is active
    /// </summary>
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Checks if token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Checks if token is revoked
    /// </summary>
    public bool IsRevoked => RevokedAt.HasValue;

    // Private constructor for EF Core
    private RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token
    /// </summary>
    public static RefreshToken Create(string ipAddress, int daysValid = 7)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new UserDomainException("IP address is required");

        if (daysValid < 1 || daysValid > 30)
            throw new UserDomainException("Refresh token validity must be between 1 and 30 days");

        return new RefreshToken
        {
            Token = GenerateRefreshToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(daysValid),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    /// <summary>
    /// Revokes the token
    /// </summary>
    public void Revoke(string reason = "Revoked", string? ipAddress = null, string? replacedByToken = null)
    {
        if (IsRevoked)
            throw new UserDomainException("Token is already revoked");

        RevokedAt = DateTime.UtcNow;
        RevokedByIp = ipAddress;
        RevokedReason = reason;
        ReplacedByToken = replacedByToken;
    }

    private static string GenerateRefreshToken()
    {
        // Generate a cryptographically secure random token
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
