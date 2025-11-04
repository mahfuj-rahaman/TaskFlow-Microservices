namespace TaskFlow.Identity.Application.DTOs;

/// <summary>
/// Data Transfer Object for AppUser
/// </summary>
public sealed record AppUserDto
{
    public Guid Id { get; init; }
    // TODO: Add properties based on AppUserEntity
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
