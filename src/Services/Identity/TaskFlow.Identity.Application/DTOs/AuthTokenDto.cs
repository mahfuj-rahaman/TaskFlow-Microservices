namespace TaskFlow.Identity.Application.DTOs;

/// <summary>
/// DTO for authentication tokens
/// </summary>
public sealed record AuthTokenDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public string TokenType { get; init; } = "Bearer";
}
