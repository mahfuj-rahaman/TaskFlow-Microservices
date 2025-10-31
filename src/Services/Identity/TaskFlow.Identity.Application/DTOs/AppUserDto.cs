namespace TaskFlow.Identity.Application.DTOs;

/// <summary>
/// Data Transfer Object for AppUser
/// </summary>
public sealed record AppUserDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
