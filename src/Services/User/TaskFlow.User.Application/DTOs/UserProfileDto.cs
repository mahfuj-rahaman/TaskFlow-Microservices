namespace TaskFlow.User.Application.DTOs;

public sealed record UserProfileDto
{
    public required Guid Id { get; init; }
    public required Guid AppUserId { get; init; }
    public required string DisplayName { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public string? PostalCode { get; init; }
    public required string Timezone { get; init; }
    public required string Language { get; init; }
    public required string Status { get; init; }
    public Dictionary<string, string> SocialLinks { get; init; } = new();
    public Dictionary<string, string> Preferences { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
