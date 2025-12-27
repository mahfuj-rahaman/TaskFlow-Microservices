namespace TaskFlow.User.Application.DTOs;

public sealed record CreateUserProfileRequest
{
    public required Guid AppUserId { get; init; }
    public required string DisplayName { get; init; }
}
