using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.Identity.Domain.Events;
using TaskFlow.Identity.Domain.Exceptions;
using TaskFlow.Identity.Domain.Enums;

namespace TaskFlow.Identity.Domain.Entities;

/// <summary>
/// AppUser aggregate root
/// </summary>
public sealed class AppUserEntity : AggregateRoot<Guid>
{
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; } = false;
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiresAt { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }
    public AppUserStatus Status { get; private set; } = AppUserStatus.Active;
    public int FailedLoginAttempts { get; private set; } = 0;
    public DateTime? LockoutEndAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    public List<string> Roles { get; private set; } = new List<string> { "User" };
    public List<string> Permissions { get; private set; } = new List<string>();
    public bool TwoFactorEnabled { get; private set; } = false;
    public string? TwoFactorSecret { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private AppUserEntity(Guid id) : base(id)
    {
    }

    /// <summary>
    /// Creates a new AppUser
    /// </summary>
    public static AppUserEntity Create()
    {
        var entity = new AppUserEntity(Guid.NewGuid())
        {
            CreatedAt = DateTime.UtcNow
        };

        entity.RaiseDomainEvent(new AppUserCreatedDomainEvent(entity.Id));

        return entity;
    }

    /// <summary>
    /// Updates AppUser information
    /// </summary>
    public void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deletes AppUser
    /// </summary>
    public void Delete()
    {
        RaiseDomainEvent(new AppUserDeletedDomainEvent(Id));
    }
}
