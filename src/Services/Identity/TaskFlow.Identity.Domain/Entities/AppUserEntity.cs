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
    public static AppUserEntity Create(
        string username,
        string email,
        string firstName,
        string lastName,
        string passwordHash)
    {
        var entity = new AppUserEntity(Guid.NewGuid())
        {
            Username = username,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            Status = AppUserStatus.Active,
            Roles = new List<string> { "User" },
            Permissions = new List<string>()
        };

        entity.RaiseDomainEvent(new AppUserCreatedDomainEvent(entity.Id));

        return entity;
    }

    /// <summary>
    /// Updates AppUser information
    /// </summary>
    public void Update(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates password
    /// </summary>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates email confirmation token
    /// </summary>
    public void GenerateEmailConfirmationToken(string token, DateTime expiresAt)
    {
        EmailConfirmationToken = token;
        EmailConfirmationTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Confirms email
    /// </summary>
    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates password reset token
    /// </summary>
    public void GeneratePasswordResetToken(string token, DateTime expiresAt)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records successful login
    /// </summary>
    public void RecordSuccessfulLogin(string ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        FailedLoginAttempts = 0;
        LockoutEndAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records failed login attempt
    /// </summary>
    public void RecordFailedLoginAttempt()
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        if (FailedLoginAttempts >= 5)
        {
            LockoutEndAt = DateTime.UtcNow.AddMinutes(30);
        }
    }

    /// <summary>
    /// Checks if account is locked out
    /// </summary>
    public bool IsLockedOut()
    {
        return LockoutEndAt.HasValue && LockoutEndAt.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Adds role to user
    /// </summary>
    public void AddRole(string role)
    {
        if (!Roles.Contains(role))
        {
            Roles.Add(role);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes role from user
    /// </summary>
    public void RemoveRole(string role)
    {
        if (Roles.Contains(role))
        {
            Roles.Remove(role);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Adds permission to user
    /// </summary>
    public void AddPermission(string permission)
    {
        if (!Permissions.Contains(permission))
        {
            Permissions.Add(permission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes permission from user
    /// </summary>
    public void RemovePermission(string permission)
    {
        if (Permissions.Contains(permission))
        {
            Permissions.Remove(permission);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Enables two-factor authentication
    /// </summary>
    public void EnableTwoFactor(string secret)
    {
        TwoFactorEnabled = true;
        TwoFactorSecret = secret;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Disables two-factor authentication
    /// </summary>
    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Changes user status
    /// </summary>
    public void ChangeStatus(AppUserStatus newStatus)
    {
        Status = newStatus;
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
