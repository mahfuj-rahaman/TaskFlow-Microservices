using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.User.Domain.Enums;
using TaskFlow.User.Domain.Events;
using TaskFlow.User.Domain.Exceptions;
using TaskFlow.User.Domain.ValueObjects;

namespace TaskFlow.User.Domain.Entities;

/// <summary>
/// AppUser aggregate root - Handles authentication and authorization
/// This is separate from UserEntity (profile data) to follow SRP
/// </summary>
public sealed class AppUser : AggregateRoot<Guid>
{
    private readonly List<string> _roles = new();
    private readonly List<string> _permissions = new();
    private readonly List<RefreshToken> _refreshTokens = new();

    // Authentication
    public string Username { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool EmailConfirmed { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiresAt { get; private set; }

    // Password Reset
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    // Security
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public DateTime? LastPasswordChangedAt { get; private set; }

    // Authorization
    public IReadOnlyCollection<string> Roles => _roles.AsReadOnly();
    public IReadOnlyCollection<string> Permissions => _permissions.AsReadOnly();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    // Profile Link
    public Guid UserEntityId { get; private set; } // Foreign key to UserEntity

    // Audit
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? LastLoginIp { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private AppUser(Guid id) : base(id) { }

    /// <summary>
    /// Creates a new AppUser (for registration)
    /// </summary>
    public static AppUser Create(
        string username,
        string email,
        string passwordHash,
        Guid userEntityId)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(username))
            throw new UserDomainException("Username is required");

        if (username.Length < 3 || username.Length > 50)
            throw new UserDomainException("Username must be between 3 and 50 characters");

        if (!IsValidUsername(username))
            throw new UserDomainException("Username can only contain letters, numbers, dots, underscores, and hyphens");

        if (string.IsNullOrWhiteSpace(email))
            throw new UserDomainException("Email is required");

        if (!IsValidEmail(email))
            throw new UserDomainException("Invalid email format");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new UserDomainException("Password hash is required");

        if (userEntityId == Guid.Empty)
            throw new UserDomainException("UserEntity ID is required");

        var appUser = new AppUser(Guid.NewGuid())
        {
            Username = username.ToLowerInvariant(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            EmailConfirmed = false,
            UserEntityId = userEntityId,
            Status = UserStatus.Active,
            FailedLoginAttempts = 0,
            TwoFactorEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        // Add default role
        appUser._roles.Add("User");

        // Generate email confirmation token
        appUser.GenerateEmailConfirmationToken();

        // Raise domain event
        appUser.RaiseDomainEvent(new AppUserCreatedDomainEvent(
            appUser.Id,
            appUser.Username,
            appUser.Email,
            appUser.UserEntityId));

        return appUser;
    }

    /// <summary>
    /// Confirms email address
    /// </summary>
    public void ConfirmEmail(string token)
    {
        if (EmailConfirmed)
            throw new UserDomainException("Email is already confirmed");

        if (string.IsNullOrWhiteSpace(EmailConfirmationToken))
            throw new UserDomainException("No email confirmation token exists");

        if (EmailConfirmationToken != token)
            throw new UserDomainException("Invalid email confirmation token");

        if (EmailConfirmationTokenExpiresAt < DateTime.UtcNow)
            throw new UserDomainException("Email confirmation token has expired");

        EmailConfirmed = true;
        EmailConfirmationToken = null;
        EmailConfirmationTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserEmailConfirmedDomainEvent(Id, Email));
    }

    /// <summary>
    /// Generates email confirmation token
    /// </summary>
    public void GenerateEmailConfirmationToken()
    {
        EmailConfirmationToken = GenerateSecureToken();
        EmailConfirmationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records successful login
    /// </summary>
    public void RecordLogin(string ipAddress)
    {
        if (Status != UserStatus.Active)
            throw new UserDomainException("User account is not active");

        if (IsLockedOut())
            throw new UserDomainException($"Account is locked until {LockoutEnd:yyyy-MM-dd HH:mm:ss}");

        if (!EmailConfirmed)
            throw new UserDomainException("Email must be confirmed before login");

        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserLoggedInDomainEvent(Id, Username, ipAddress));
    }

    /// <summary>
    /// Records failed login attempt
    /// </summary>
    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        // Lock account after 5 failed attempts
        if (FailedLoginAttempts >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(30);
            RaiseDomainEvent(new AppUserLockedOutDomainEvent(Id, Username, LockoutEnd.Value));
        }
    }

    /// <summary>
    /// Changes password
    /// </summary>
    public void ChangePassword(string currentPasswordHash, string newPasswordHash)
    {
        if (PasswordHash != currentPasswordHash)
            throw new UserDomainException("Current password is incorrect");

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new UserDomainException("New password hash is required");

        PasswordHash = newPasswordHash;
        LastPasswordChangedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Invalidate all refresh tokens
        foreach (var token in _refreshTokens)
        {
            token.Revoke();
        }

        RaiseDomainEvent(new AppUserPasswordChangedDomainEvent(Id, Username));
    }

    /// <summary>
    /// Initiates password reset
    /// </summary>
    public void InitiatePasswordReset()
    {
        PasswordResetToken = GenerateSecureToken();
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserPasswordResetRequestedDomainEvent(Id, Email, PasswordResetToken));
    }

    /// <summary>
    /// Resets password with token
    /// </summary>
    public void ResetPassword(string token, string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(PasswordResetToken))
            throw new UserDomainException("No password reset token exists");

        if (PasswordResetToken != token)
            throw new UserDomainException("Invalid password reset token");

        if (PasswordResetTokenExpiresAt < DateTime.UtcNow)
            throw new UserDomainException("Password reset token has expired");

        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new UserDomainException("New password hash is required");

        PasswordHash = newPasswordHash;
        LastPasswordChangedAt = DateTime.UtcNow;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        UpdatedAt = DateTime.UtcNow;

        // Invalidate all refresh tokens
        foreach (var token in _refreshTokens)
        {
            token.Revoke();
        }

        RaiseDomainEvent(new AppUserPasswordResetDomainEvent(Id, Username));
    }

    /// <summary>
    /// Adds a role
    /// </summary>
    public void AddRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new UserDomainException("Role name is required");

        var normalizedRole = role.Trim();
        if (_roles.Contains(normalizedRole, StringComparer.OrdinalIgnoreCase))
            return; // Already has role

        _roles.Add(normalizedRole);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserRoleAddedDomainEvent(Id, Username, normalizedRole));
    }

    /// <summary>
    /// Removes a role
    /// </summary>
    public void RemoveRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new UserDomainException("Role name is required");

        var normalizedRole = role.Trim();
        if (!_roles.Remove(normalizedRole))
            return; // Role didn't exist

        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserRoleRemovedDomainEvent(Id, Username, normalizedRole));
    }

    /// <summary>
    /// Adds a permission
    /// </summary>
    public void AddPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new UserDomainException("Permission name is required");

        var normalizedPermission = permission.Trim();
        if (_permissions.Contains(normalizedPermission, StringComparer.OrdinalIgnoreCase))
            return;

        _permissions.Add(normalizedPermission);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a permission
    /// </summary>
    public void RemovePermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new UserDomainException("Permission name is required");

        _permissions.Remove(permission.Trim());
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    public bool HasRole(string role)
    {
        return _roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if user has a specific permission
    /// </summary>
    public bool HasPermission(string permission)
    {
        return _permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds a refresh token
    /// </summary>
    public void AddRefreshToken(RefreshToken refreshToken)
    {
        if (refreshToken == null)
            throw new UserDomainException("Refresh token is required");

        // Remove expired tokens
        _refreshTokens.RemoveAll(t => !t.IsActive);

        // Limit to 5 active tokens per user
        while (_refreshTokens.Count >= 5)
        {
            var oldestToken = _refreshTokens.OrderBy(t => t.CreatedAt).First();
            _refreshTokens.Remove(oldestToken);
        }

        _refreshTokens.Add(refreshToken);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes a refresh token
    /// </summary>
    public void RevokeRefreshToken(string token, string reason = "Revoked")
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        if (refreshToken == null)
            throw new UserDomainException("Refresh token not found");

        refreshToken.Revoke(reason);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes all refresh tokens
    /// </summary>
    public void RevokeAllRefreshTokens(string reason = "All tokens revoked")
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(reason);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Enables two-factor authentication
    /// </summary>
    public void EnableTwoFactor(string secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new UserDomainException("Two-factor secret is required");

        TwoFactorEnabled = true;
        TwoFactorSecret = secret;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserTwoFactorEnabledDomainEvent(Id, Username));
    }

    /// <summary>
    /// Disables two-factor authentication
    /// </summary>
    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        TwoFactorSecret = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserTwoFactorDisabledDomainEvent(Id, Username));
    }

    /// <summary>
    /// Deactivates the account
    /// </summary>
    public void Deactivate()
    {
        if (Status == UserStatus.Inactive)
            throw new UserDomainException("Account is already inactive");

        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        // Revoke all tokens
        RevokeAllRefreshTokens("Account deactivated");

        RaiseDomainEvent(new AppUserDeactivatedDomainEvent(Id, Username));
    }

    /// <summary>
    /// Activates the account
    /// </summary>
    public void Activate()
    {
        if (Status == UserStatus.Active)
            throw new UserDomainException("Account is already active");

        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new AppUserActivatedDomainEvent(Id, Username));
    }

    /// <summary>
    /// Suspends the account
    /// </summary>
    public void Suspend(string reason)
    {
        if (Status == UserStatus.Suspended)
            throw new UserDomainException("Account is already suspended");

        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        // Revoke all tokens
        RevokeAllRefreshTokens($"Account suspended: {reason}");

        RaiseDomainEvent(new AppUserSuspendedDomainEvent(Id, Username, reason));
    }

    /// <summary>
    /// Checks if account is locked out
    /// </summary>
    public bool IsLockedOut()
    {
        return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    /// <summary>
    /// Unlocks the account
    /// </summary>
    public void Unlock()
    {
        LockoutEnd = null;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    // Helper methods
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidUsername(string username)
    {
        // Only allow alphanumeric, dots, underscores, and hyphens
        return username.All(c => char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-');
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray()) +
               Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}
