namespace TaskFlow.User.Application.DTOs;

public sealed record UpdateUserProfileRequest
{
    public required string DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? PhoneNumber { get; init; }
}
