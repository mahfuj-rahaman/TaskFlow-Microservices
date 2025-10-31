# Identity Feature - Complete Specification

## Overview

Production-ready Identity system for TaskFlow microservices platform. Handles authentication, authorization, user onboarding, and password management.

## Architecture

### Domain Model

#### Entities

**AppUser** (Aggregate Root):
- Primary entity for authentication/authorization
- Links to UserEntity (profile data) via UserEntityId
- Handles credentials, roles, permissions, tokens
- Status: COMPLETED ✅

**UserEntity** (Existing):
- Profile information (First Name, Last Name, DOB, etc.)
- Status: COMPLETED ✅

#### Value Objects

**RefreshToken**:
- Token, ExpiresAt, CreatedAt, CreatedByIp
- RevokedAt, RevokedByIp, RevokedReason, ReplacedByToken
- Status: COMPLETED ✅

### Application Layer

#### DTOs (Data Transfer Objects)

**Authentication DTOs**:
```csharp
// Request DTOs
public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    DateTime DateOfBirth);

public record LoginRequest(
    string UsernameOrEmail,
    string Password,
    string? TwoFactorCode);

public record RefreshTokenRequest(
    string RefreshToken);

public record ConfirmEmailRequest(
    string Email,
    string Token);

// Response DTOs
public record AuthenticationResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfileDto User);

public record UserProfileDto(
    Guid Id,
    string Username,
    string Email,
    bool EmailConfirmed,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    UserStatus Status,
    List<string> Roles,
    List<string> Permissions);
```

**Password Management DTOs**:
```csharp
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public record ForgotPasswordRequest(
    string Email);

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);
```

**User Management DTOs**:
```csharp
public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth);

public record AddRoleRequest(
    Guid UserId,
    string Role);

public record RemoveRoleRequest(
    Guid UserId,
    string Role);
```

#### Commands (Write Operations)

**Authentication Commands**:

1. **RegisterCommand**
   - Creates AppUser + UserEntity
   - Hashes password
   - Generates email confirmation token
   - Sends confirmation email
   - Returns authentication response

2. **LoginCommand**
   - Validates credentials
   - Checks account status, lockout, email confirmation
   - Records failed attempts
   - Generates JWT + refresh token
   - Records login
   - Returns authentication response

3. **RefreshTokenCommand**
   - Validates refresh token
   - Revokes old token
   - Generates new token pair
   - Returns authentication response

4. **ConfirmEmailCommand**
   - Validates confirmation token
   - Marks email as confirmed
   - Returns success

5. **LogoutCommand**
   - Revokes refresh token
   - Returns success

**Password Management Commands**:

6. **ChangePasswordCommand**
   - Validates current password
   - Hashes new password
   - Updates password
   - Revokes all refresh tokens
   - Returns success

7. **ForgotPasswordCommand**
   - Generates reset token
   - Sends reset email
   - Returns success

8. **ResetPasswordCommand**
   - Validates reset token
   - Hashes new password
   - Updates password
   - Revokes all refresh tokens
   - Returns success

**User Management Commands**:

9. **UpdateProfileCommand**
   - Updates UserEntity
   - Returns updated profile

10. **AddRoleCommand**
    - Adds role to AppUser
    - Grants role permissions
    - Returns success

11. **RemoveRoleCommand**
    - Removes role from AppUser
    - Revokes role permissions
    - Returns success

12. **DeactivateUserCommand**
    - Deactivates AppUser + UserEntity
    - Revokes all tokens
    - Returns success

13. **ActivateUserCommand**
    - Activates AppUser + UserEntity
    - Unlocks account
    - Returns success

#### Queries (Read Operations)

1. **GetUserProfileQuery**
   - Returns complete user profile (AppUser + UserEntity)

2. **GetAllUsersQuery**
   - Returns paginated list of users
   - Supports filtering, sorting

3. **GetUserByIdQuery**
   - Returns specific user by ID

4. **GetUserByUsernameQuery**
   - Returns user by username

5. **GetUserByEmailQuery**
   - Returns user by email

6. **GetUserRolesQuery**
   - Returns user's roles

7. **GetUserPermissionsQuery**
   - Returns user's permissions (role-based + custom)

#### Validators

**FluentValidation validators for all commands**:

```csharp
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .Matches("^[a-zA-Z0-9._-]+$")
            .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain uppercase")
            .Matches("[a-z]").WithMessage("Password must contain lowercase")
            .Matches("[0-9]").WithMessage("Password must contain digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain special character");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today)
            .GreaterThan(DateTime.Today.AddYears(-120));
    }
}
```

### Infrastructure Layer

#### Services

**IPasswordHasher**:
```csharp
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

// Implementation using BCrypt
public class BcryptPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, 12);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
```

**IJwtTokenService**:
```csharp
public interface IJwtTokenService
{
    string GenerateAccessToken(AppUser user, UserEntity userEntity);
    ClaimsPrincipal? ValidateToken(string token);
}

// Implementation
public class JwtTokenService : IJwtTokenService
{
    // JWT generation with claims:
    // - sub (subject): AppUserId
    // - email
    // - username
    // - roles (multiple claims)
    // - permissions (multiple claims)
    // - profile_id: UserEntityId
    // - jti (JWT ID)
    // - iat (issued at)
    // - exp (expiration): 15 minutes

    public string GenerateAccessToken(AppUser user, UserEntity userEntity)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
            new("username", user.Username),
            new("profile_id", user.UserEntityId.ToString()),
            new("email_confirmed", user.EmailConfirmed.ToString()),
            new("full_name", userEntity.GetFullName())
        };

        // Add roles
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions
        foreach (var permission in user.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        // Generate token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

**IEmailService**:
```csharp
public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    Task SendPasswordResetAsync(string email, string resetLink);
    Task SendWelcomeEmailAsync(string email, string firstName);
}
```

#### Repositories

**IAppUserRepository**:
```csharp
public interface IAppUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AppUser?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<PagedList<AppUser>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(AppUser appUser, CancellationToken ct = default);
    void Update(AppUser appUser);
    void Delete(AppUser appUser);
}
```

**IUserEntityRepository**:
```csharp
public interface IUserEntityRepository
{
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(UserEntity user, CancellationToken ct = default);
    void Update(UserEntity user);
    void Delete(UserEntity user);
}
```

#### EF Core Configuration

**AppUserConfiguration**:
```csharp
public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        // Owned collection for roles
        builder.Property(x => x.Roles)
            .HasColumnName("Roles")
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        // Owned collection for permissions
        builder.Property(x => x.Permissions)
            .HasColumnName("Permissions")
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        // Owned collection for refresh tokens
        builder.OwnsMany(x => x.RefreshTokens, rt =>
        {
            rt.ToTable("RefreshTokens");
            rt.WithOwner().HasForeignKey("AppUserId");
            rt.Property(x => x.Token).IsRequired().HasMaxLength(500);
            rt.Property(x => x.CreatedByIp).IsRequired().HasMaxLength(50);
            rt.Property(x => x.RevokedByIp).HasMaxLength(50);
            rt.HasIndex(x => x.Token);
        });

        // Foreign key to UserEntity
        builder.HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<AppUser>(x => x.UserEntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### API Layer

#### Controllers

**AuthController**:
```csharp
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request);

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request);

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request);

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout();

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request);

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request);

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request);

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request);
}
```

**UsersController**:
```csharp
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    public async Task<IActionResult> GetMyProfile();

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request);

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(PagedList<UserProfileDto>), 200)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10);

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById(Guid id);

    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddRole(Guid id, [FromBody] AddRoleRequest request);

    [HttpDelete("{id:guid}/roles")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RemoveRole(Guid id, [FromBody] RemoveRoleRequest request);

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> DeactivateUser(Guid id);

    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ActivateUser(Guid id);
}
```

### Configuration

**appsettings.json**:
```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key-here-min-32-characters",
    "Issuer": "TaskFlow.API",
    "Audience": "TaskFlow.Client",
    "ExpirationMinutes": 15
  },
  "RefreshTokenSettings": {
    "ExpirationDays": 7
  },
  "PasswordPolicy": {
    "RequireDigit": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireNonAlphanumeric": true,
    "MinimumLength": 8
  }
}
```

## Implementation Steps

1. ✅ Domain Layer (COMPLETED)
   - AppUser entity
   - RefreshToken value object
   - Domain events
   - Enums (UserRole, Permission)

2. Application Layer (NEXT)
   - DTOs
   - Commands + Handlers + Validators
   - Queries + Handlers
   - Repository interfaces

3. Infrastructure Layer
   - Password hasher service
   - JWT token service
   - Email service
   - Repository implementations
   - EF Core configurations

4. API Layer
   - Auth controller
   - Users controller
   - Middleware (JWT authentication)

5. Testing
   - Unit tests (Domain, Application)
   - Integration tests (API)

## Security Considerations

- ✅ Passwords hashed with BCrypt (work factor 12)
- ✅ JWT tokens expire in 15 minutes
- ✅ Refresh tokens expire in 7 days
- ✅ Account lockout after 5 failed attempts (30 min lockout)
- ✅ Email confirmation required
- ✅ Secure token generation for email/password reset
- ✅ Revoke all tokens on password change
- ✅ Limit 5 active refresh tokens per user
- ✅ Role-based and permission-based authorization
- ✅ Two-factor authentication support (optional)
- ✅ IP address tracking for security audit

## User Onboarding Flow

1. User submits registration → RegisterCommand
2. System creates UserEntity (profile)
3. System creates AppUser (credentials)
4. System sends email confirmation
5. User clicks confirmation link → ConfirmEmailCommand
6. User can now login → LoginCommand
7. System returns JWT + refresh token
8. User accesses protected resources with JWT
9. JWT expires → RefreshTokenCommand
10. System returns new JWT + refresh token

## Password Reset Flow

1. User forgets password → ForgotPasswordCommand
2. System generates reset token
3. System sends reset email
4. User clicks reset link → ResetPasswordCommand
5. System validates token
6. System updates password
7. System revokes all refresh tokens
8. User must login again

## Notes

- AppUser and UserEntity are separate for SRP (Single Responsibility Principle)
- AppUser handles auth/authz (security concerns)
- UserEntity handles profile (business concerns)
- This allows independent evolution and testing
- Both share same lifecycle (created together, deleted together via cascade)
