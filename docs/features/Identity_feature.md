# Identity Feature Specification (AppUser)

**Service**: Identity
**Created**: 2025-11-04
**Status**: Draft
**Version**: 1.0

---

## 1. Overview

### Purpose
Complete production-ready Identity system managing authentication, authorization, and user roles. The **AppUser** entity acts as the central identity hub that all other services reference via `AppUserId` (IdentityUserId).

### Key Concepts
- **Central Identity**: AppUser is the single source of truth for authentication
- **Role-Based Access**: 3-tier role system (SuperAdmin, Admin, User)
- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **Foreign Key Hub**: Other services reference AppUser via IdentityUserId/AppUserId

---

## 2. Domain Model

### 2.1 AppUser Entity (Aggregate Root)

```csharp
public sealed class AppUser : AggregateRoot<Guid>
{
    // Basic Identity
    public string Username { get; private set; }          // Unique, 3-50 chars
    public string Email { get; private set; }             // Unique, valid email
    public string PasswordHash { get; private set; }      // BCrypt hashed

    // Email Verification
    public bool EmailConfirmed { get; private set; }      // Default: false
    public string? EmailConfirmationToken { get; private set; }
    public DateTime? EmailConfirmationTokenExpiry { get; private set; }

    // Password Reset
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // Role & Permissions
    public AppUserRole Role { get; private set; }         // SuperAdmin, Admin, User
    public List<string> Permissions { get; private set; } // Fine-grained permissions

    // Security
    public bool IsActive { get; private set; }            // Default: true
    public bool IsLocked { get; private set; }            // Account lockout
    public int FailedLoginAttempts { get; private set; }  // Track failed logins
    public DateTime? LockoutEnd { get; private set; }     // Auto-unlock time
    public DateTime? LastLoginAt { get; private set; }    // Track last login

    // Refresh Tokens
    public List<RefreshToken> RefreshTokens { get; private set; }

    // Two-Factor Authentication (Future)
    public bool TwoFactorEnabled { get; private set; }    // Default: false
    public string? TwoFactorSecret { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }          // For admin-created users
}
```

### 2.2 RefreshToken Value Object

```csharp
public sealed class RefreshToken
{
    public string Token { get; private set; }             // Unique token
    public DateTime ExpiresAt { get; private set; }       // 7 days validity
    public DateTime CreatedAt { get; private set; }
    public string CreatedByIp { get; private set; }       // Track IP
    public DateTime? RevokedAt { get; private set; }      // If revoked
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }  // Token rotation

    public bool IsActive => RevokedAt == null && !IsExpired;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
```

### 2.3 AppUserRole Enum

```csharp
public enum AppUserRole
{
    User = 0,           // Default role for all registered users
    Admin = 1,          // Can manage users, assigned by SuperAdmin
    SuperAdmin = 2      // Full system access, created by seeder
}
```

---

## 3. Business Rules

### 3.1 Registration Rules
1. **Username**: 3-50 characters, alphanumeric + underscore, unique
2. **Email**: Valid email format, unique, case-insensitive
3. **Password**: Minimum 8 characters, must contain uppercase, lowercase, digit, special char
4. **Default Role**: All registered users get `User` role automatically
5. **Email Confirmation**: Account created but email must be confirmed before full access
6. **Email Confirmation Token**: Valid for 24 hours

### 3.2 Authentication Rules
1. **Email Must Be Confirmed**: Cannot login until email is confirmed
2. **Account Must Be Active**: IsActive must be true
3. **Account Not Locked**: IsLocked must be false or LockoutEnd has passed
4. **Password Verification**: BCrypt verify against stored hash
5. **Failed Login Tracking**: Increment FailedLoginAttempts on wrong password
6. **Account Lockout**: Lock account for 30 minutes after 5 failed attempts
7. **Success Login**: Reset FailedLoginAttempts to 0, update LastLoginAt

### 3.3 JWT Token Rules
1. **Access Token**: Valid for 15 minutes
2. **Refresh Token**: Valid for 7 days
3. **Token Claims**: Include UserId, Username, Email, Role, Permissions
4. **Token Rotation**: New refresh token issued on each refresh, old token invalidated
5. **Revocation**: Refresh tokens can be revoked (logout, security breach)

### 3.4 Password Reset Rules
1. **Reset Token**: Valid for 1 hour
2. **Token Format**: Cryptographically secure random token
3. **Single Use**: Token invalidated after successful password reset
4. **Email Required**: Send reset link to registered email
5. **Password Change**: Must meet password strength requirements

### 3.5 Role Management Rules
1. **User Role**: Assigned automatically on registration
2. **Admin Role**: Can ONLY be assigned by SuperAdmin
3. **SuperAdmin Role**: CANNOT be assigned manually, created by database seeder
4. **Role Downgrade**: SuperAdmin can demote Admin to User
5. **Self-Demotion**: Users cannot change their own role
6. **SuperAdmin Protection**: SuperAdmin role cannot be removed or changed

### 3.6 Account Security Rules
1. **Account Activation**: Admins can deactivate user accounts (IsActive = false)
2. **Account Lock**: System automatically locks after failed login attempts
3. **Manual Lock**: Admins can manually lock accounts
4. **Unlock**: Accounts auto-unlock after LockoutEnd time passes
5. **Permission Check**: All operations verify user has required permission

---

## 4. Operations (Commands & Queries)

### 4.1 Commands (Write Operations)

#### Register
```csharp
public record RegisterCommand
{
    public string Username { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public string ConfirmPassword { get; init; }
}
// Returns: AppUserId, EmailConfirmationToken
// Side Effects: Sends email confirmation link
```

#### Login
```csharp
public record LoginCommand
{
    public string EmailOrUsername { get; init; }
    public string Password { get; init; }
    public string IpAddress { get; init; }
}
// Returns: AccessToken, RefreshToken, ExpiresAt
// Side Effects: Creates refresh token, updates LastLoginAt
```

#### RefreshToken
```csharp
public record RefreshTokenCommand
{
    public string RefreshToken { get; init; }
    public string IpAddress { get; init; }
}
// Returns: New AccessToken, New RefreshToken
// Side Effects: Revokes old refresh token, creates new one
```

#### Logout
```csharp
public record LogoutCommand
{
    public Guid AppUserId { get; init; }
    public string? RefreshToken { get; init; }
    public string IpAddress { get; init; }
}
// Returns: Success
// Side Effects: Revokes refresh token(s)
```

#### ConfirmEmail
```csharp
public record ConfirmEmailCommand
{
    public string Email { get; init; }
    public string Token { get; init; }
}
// Returns: Success
// Side Effects: Sets EmailConfirmed = true
```

#### ForgotPassword
```csharp
public record ForgotPasswordCommand
{
    public string Email { get; init; }
}
// Returns: Success (always, for security)
// Side Effects: Generates reset token, sends email
```

#### ResetPassword
```csharp
public record ResetPasswordCommand
{
    public string Email { get; init; }
    public string Token { get; init; }
    public string NewPassword { get; init; }
    public string ConfirmPassword { get; init; }
}
// Returns: Success
// Side Effects: Changes password, invalidates reset token
```

#### ChangePassword
```csharp
public record ChangePasswordCommand
{
    public Guid AppUserId { get; init; }
    public string CurrentPassword { get; init; }
    public string NewPassword { get; init; }
    public string ConfirmPassword { get; init; }
}
// Returns: Success
// Side Effects: Changes password, revokes all refresh tokens
```

#### AssignRole (Admin Only)
```csharp
public record AssignRoleCommand
{
    public Guid AppUserId { get; init; }
    public AppUserRole NewRole { get; init; }
    public Guid PerformedBy { get; init; }      // Must be SuperAdmin
}
// Returns: Success
// Side Effects: Changes user role
// Authorization: SuperAdmin only
```

#### ActivateAccount (Admin Only)
```csharp
public record ActivateAccountCommand
{
    public Guid AppUserId { get; init; }
    public Guid PerformedBy { get; init; }
}
// Returns: Success
// Side Effects: Sets IsActive = true
// Authorization: Admin or SuperAdmin
```

#### DeactivateAccount (Admin Only)
```csharp
public record DeactivateAccountCommand
{
    public Guid AppUserId { get; init; }
    public Guid PerformedBy { get; init; }
    public string Reason { get; init; }
}
// Returns: Success
// Side Effects: Sets IsActive = false, revokes tokens
// Authorization: Admin or SuperAdmin
```

#### LockAccount (Admin Only)
```csharp
public record LockAccountCommand
{
    public Guid AppUserId { get; init; }
    public Guid PerformedBy { get; init; }
    public TimeSpan? LockoutDuration { get; init; }  // null = indefinite
}
// Returns: Success
// Side Effects: Sets IsLocked = true, sets LockoutEnd
// Authorization: Admin or SuperAdmin
```

#### UnlockAccount (Admin Only)
```csharp
public record UnlockAccountCommand
{
    public Guid AppUserId { get; init; }
    public Guid PerformedBy { get; init; }
}
// Returns: Success
// Side Effects: Sets IsLocked = false, resets FailedLoginAttempts
// Authorization: Admin or SuperAdmin
```

### 4.2 Queries (Read Operations)

#### GetAppUserById
```csharp
public record GetAppUserByIdQuery(Guid AppUserId);
// Returns: AppUserDto (without sensitive data)
```

#### GetAppUserByEmail
```csharp
public record GetAppUserByEmailQuery(string Email);
// Returns: AppUserDto (without sensitive data)
```

#### GetAppUserByUsername
```csharp
public record GetAppUserByUsernameQuery(string Username);
// Returns: AppUserDto (without sensitive data)
```

#### GetAllAppUsers (Admin Only)
```csharp
public record GetAllAppUsersQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public AppUserRole? FilterByRole { get; init; }
    public bool? FilterByActive { get; init; }
}
// Returns: PagedList<AppUserDto>
// Authorization: Admin or SuperAdmin
```

#### GetUserRefreshTokens
```csharp
public record GetUserRefreshTokensQuery(Guid AppUserId);
// Returns: List<RefreshTokenDto> (active tokens only)
```

#### ValidateToken
```csharp
public record ValidateTokenQuery(string Token);
// Returns: TokenValidationResult
```

---

## 5. Domain Events

```csharp
public record AppUserRegisteredDomainEvent(Guid AppUserId, string Email, string Username, DateTime RegisteredAt);

public record AppUserLoggedInDomainEvent(Guid AppUserId, string IpAddress, DateTime LoggedInAt);

public record AppUserLoggedOutDomainEvent(Guid AppUserId, DateTime LoggedOutAt);

public record EmailConfirmedDomainEvent(Guid AppUserId, DateTime ConfirmedAt);

public record PasswordResetRequestedDomainEvent(Guid AppUserId, DateTime RequestedAt);

public record PasswordChangedDomainEvent(Guid AppUserId, DateTime ChangedAt);

public record RoleAssignedDomainEvent(Guid AppUserId, AppUserRole OldRole, AppUserRole NewRole, Guid AssignedBy, DateTime AssignedAt);

public record AccountDeactivatedDomainEvent(Guid AppUserId, string Reason, Guid DeactivatedBy, DateTime DeactivatedAt);

public record AccountActivatedDomainEvent(Guid AppUserId, Guid ActivatedBy, DateTime ActivatedAt);

public record AccountLockedDomainEvent(Guid AppUserId, string Reason, DateTime LockedAt, DateTime? LockoutEnd);

public record AccountUnlockedDomainEvent(Guid AppUserId, Guid UnlockedBy, DateTime UnlockedAt);

public record RefreshTokenCreatedDomainEvent(Guid AppUserId, string Token, DateTime CreatedAt, DateTime ExpiresAt);

public record RefreshTokenRevokedDomainEvent(Guid AppUserId, string Token, DateTime RevokedAt);
```

---

## 6. Integration with Other Services

### 6.1 Foreign Key Reference
- **User Service**: References `AppUserId` as `IdentityUserId` (FK)
- **Admin Service**: References `AppUserId` as `IdentityUserId` (FK)
- **Task Service**: References `AppUserId` indirectly via User/Admin

### 6.2 Service Communication
```csharp
// User Service calls Identity to validate
public interface IIdentityService
{
    Task<bool> ValidateTokenAsync(string token);
    Task<AppUserDto?> GetAppUserByIdAsync(Guid appUserId);
}
```

---

## 7. Data Seeding

### SuperAdmin Seeding (Required)
```csharp
// Database seeder creates default SuperAdmin on first run
var superAdmin = AppUser.CreateSuperAdmin(
    username: "superadmin",
    email: "superadmin@taskflow.com",
    password: "SuperAdmin@123" // Should be env variable
);
superAdmin.ConfirmEmail(); // Pre-confirmed
```

---

## 8. Validation Rules

### RegisterCommand Validation
- Username: Required, 3-50 chars, alphanumeric + underscore
- Email: Required, valid email format, unique
- Password: Required, min 8 chars, uppercase, lowercase, digit, special char
- ConfirmPassword: Must match Password

### LoginCommand Validation
- EmailOrUsername: Required
- Password: Required
- IpAddress: Required, valid IP format

### ChangePassword Validation
- CurrentPassword: Required
- NewPassword: Required, min 8 chars, strength requirements
- ConfirmPassword: Must match NewPassword
- NewPassword must be different from CurrentPassword

### AssignRole Validation
- AppUserId: Must exist
- NewRole: Must be valid enum value
- PerformedBy: Must be SuperAdmin
- Cannot assign SuperAdmin role
- Cannot change own role

---

## 9. Security Considerations

### Password Security
- BCrypt hashing with work factor 12
- Passwords never logged or exposed in API responses
- Password reset tokens are single-use

### Token Security
- JWT signed with HMAC SHA256
- Refresh tokens stored hashed in database
- Token rotation on every refresh
- IP address tracking for tokens

### Rate Limiting
- Login: 5 attempts per minute per IP
- Register: 3 attempts per hour per IP
- Password Reset: 3 requests per hour per email

### CORS & HTTPS
- HTTPS required in production
- CORS configured for allowed origins only

---

## 10. Database Schema

### AppUsers Table
```sql
CREATE TABLE AppUsers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    EmailConfirmed BIT NOT NULL DEFAULT 0,
    EmailConfirmationToken NVARCHAR(255),
    EmailConfirmationTokenExpiry DATETIME2,
    PasswordResetToken NVARCHAR(255),
    PasswordResetTokenExpiry DATETIME2,
    Role INT NOT NULL DEFAULT 0,
    Permissions NVARCHAR(MAX), -- JSON array
    IsActive BIT NOT NULL DEFAULT 1,
    IsLocked BIT NOT NULL DEFAULT 0,
    FailedLoginAttempts INT NOT NULL DEFAULT 0,
    LockoutEnd DATETIME2,
    LastLoginAt DATETIME2,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorSecret NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2,
    CreatedBy UNIQUEIDENTIFIER,

    INDEX IX_AppUsers_Email (Email),
    INDEX IX_AppUsers_Username (Username),
    INDEX IX_AppUsers_Role (Role)
);
```

### RefreshTokens Table
```sql
CREATE TABLE RefreshTokens (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    AppUserId UNIQUEIDENTIFIER NOT NULL,
    Token NVARCHAR(255) NOT NULL UNIQUE,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 NOT NULL,
    CreatedByIp NVARCHAR(45) NOT NULL,
    RevokedAt DATETIME2,
    RevokedByIp NVARCHAR(45),
    ReplacedByToken NVARCHAR(255),

    FOREIGN KEY (AppUserId) REFERENCES AppUsers(Id) ON DELETE CASCADE,
    INDEX IX_RefreshTokens_Token (Token),
    INDEX IX_RefreshTokens_AppUserId (AppUserId)
);
```

---

## 11. API Endpoints

### Public Endpoints (No Auth Required)
- `POST /api/v1/identity/register` - Register new user
- `POST /api/v1/identity/login` - Login
- `POST /api/v1/identity/refresh-token` - Refresh access token
- `POST /api/v1/identity/confirm-email` - Confirm email
- `POST /api/v1/identity/forgot-password` - Request password reset
- `POST /api/v1/identity/reset-password` - Reset password

### Authenticated Endpoints (Requires JWT)
- `POST /api/v1/identity/logout` - Logout
- `POST /api/v1/identity/change-password` - Change password
- `GET /api/v1/identity/me` - Get current user info
- `GET /api/v1/identity/refresh-tokens` - Get active refresh tokens

### Admin Endpoints (Requires Admin or SuperAdmin Role)
- `GET /api/v1/identity/users` - Get all users (paginated)
- `GET /api/v1/identity/users/{id}` - Get user by ID
- `POST /api/v1/identity/users/{id}/activate` - Activate account
- `POST /api/v1/identity/users/{id}/deactivate` - Deactivate account
- `POST /api/v1/identity/users/{id}/lock` - Lock account
- `POST /api/v1/identity/users/{id}/unlock` - Unlock account

### SuperAdmin Endpoints (Requires SuperAdmin Role Only)
- `POST /api/v1/identity/users/{id}/assign-role` - Assign/change role

---

## 12. Error Handling

### Error Codes
- `IDENTITY_001`: Invalid credentials
- `IDENTITY_002`: Email not confirmed
- `IDENTITY_003`: Account locked
- `IDENTITY_004`: Account deactivated
- `IDENTITY_005`: Invalid token
- `IDENTITY_006`: Token expired
- `IDENTITY_007`: Username already exists
- `IDENTITY_008`: Email already exists
- `IDENTITY_009`: Weak password
- `IDENTITY_010`: Invalid email format
- `IDENTITY_011`: Unauthorized role assignment
- `IDENTITY_012`: Cannot change own role
- `IDENTITY_013`: Invalid refresh token

---

## 13. Testing Strategy

### Unit Tests
- AppUser entity business logic
- Password hashing and verification
- Token generation and validation
- Role assignment logic
- Account lockout logic

### Integration Tests
- Registration flow
- Login flow with email confirmation
- Password reset flow
- Token refresh flow
- Role assignment by SuperAdmin
- Account activation/deactivation

### E2E Tests
- Complete user registration and login
- Forgot password and reset
- Token refresh and logout
- Admin role management

---

## 14. Performance Considerations

- **Caching**: Cache user roles and permissions (5 min TTL)
- **Indexing**: Email, Username, Role columns indexed
- **Token Cleanup**: Background job to delete expired refresh tokens
- **Password Hashing**: Async BCrypt to avoid blocking

---

## 15. Future Enhancements

- Two-Factor Authentication (TOTP)
- Social Login (Google, GitHub, Microsoft)
- Password history (prevent reuse of last 5 passwords)
- Session management (view/revoke active sessions)
- Security audit log (track all sensitive operations)
- Email notification for suspicious activities
- Biometric authentication support

---

**Ready for Implementation**: ✅
**Dependencies**: BuildingBlocks.Common, BuildingBlocks.CQRS
**Database**: PostgreSQL (TaskFlow_Identity)
