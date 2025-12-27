namespace TaskFlow.Identity.Application.DTOs;

/// <summary>
/// DTO for authentication response
/// </summary>
public sealed record AuthResponse
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required List<string> Roles { get; init; }
    public required AuthTokenDto Token { get; init; }
}
