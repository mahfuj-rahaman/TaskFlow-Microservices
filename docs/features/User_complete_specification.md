# User Feature - Complete Specification

## Overview

Production-ready User management system for TaskFlow microservices platform with **self-registration**, **recursive endless hierarchy** of Master-SubUser relationships, invitation-based access control, and task management.

**Key Principles**:
- ✅ All users must register themselves (self-service registration)
- ✅ **Two-stage user creation**:
  1. Registration → Creates AppUser (Identity) → Email sent
  2. Email confirmation → AppUser.EmailConfirmed = true
  3. First login → Redirects to profile completion
  4. Profile completion → Creates UserEntity → User fully activated
- ✅ UserEntity created ONLY after first successful login + profile completion
- ✅ Profile completion is MANDATORY (cannot skip)
- ✅ Any registered user can be both Master AND SubUser simultaneously
- ✅ Hierarchy is **ONE-DIRECTIONAL ONLY**: A → B → C → D → ... → ∞
- ❌ NO bidirectional: A → B AND B → A (NOT ALLOWED)
- ❌ NO cycles: A → B → C → A (NOT ALLOWED)
- ✅ Tree/DAG structure with validation to prevent cycles
- ✅ Invitations sent via email to anyone (registered or unregistered)
- ✅ If invitee has AppUser (Identity) → can complete profile and accept
- ✅ If invitee doesn't have AppUser → must register, confirm email, login, complete profile, then accept
- ℹ️ Admin users handled separately (separate AdminUser entity, not in User hierarchy)

## Architecture

### Domain Model

#### Core Concepts

**1. User Hierarchy (One-Directional Tree/DAG)**:
- **AppUser** (Identity layer) - Handles authentication/authorization (already exists)
- **UserEntity** (Business layer) - Handles profile and relationships
- **ANY user can be BOTH Master AND SubUser at the same time**
- **Hierarchy is ONE-DIRECTIONAL**: A → B → C → D → E → ... (tree/DAG structure)
- ❌ **NO cycles**: A → B → C → A is INVALID (must prevent)
- ❌ **NO bidirectional**: A → B AND B → A is INVALID (must prevent)
- ✅ **Endless depth**: A → B → C → D → E → ... (no depth limit)
- **Validation required**: Before creating relation, check for existing path in reverse direction
- **No distinction**: All users are equal, roles determined by relationships only

**2. Relationships (N:M Self-Referencing One-Directional)**:
- One UserEntity links to one AppUser via `AppUserId` (1:1)
- UserEntity has **self-referencing N:M relationship** via `UserMasterSubUserRelation`
- One user can have MANY MasterUsers (users who invited them)
- One user can have MANY SubUsers (users they invited)
- Same user can be in both roles simultaneously
- **Directional enforcement**: If A → B exists, then B → A is BLOCKED
- **Cycle prevention**: If path exists A → ... → C, then C → A is BLOCKED
- No depth restrictions on hierarchy
- SubUsers can access and update tasks based on granted permissions
- **Admin management**: Separate AdminUser entity (not part of User hierarchy)

**3. Task Management**:
- User can create tasks
- User can assign tasks to other users
- User can receive task assignments
- User can update task status
- SubUser can view/update MasterUser's tasks
- MasterUser can view SubUser's activity

#### Entities

**UserEntity** (Aggregate Root - Enhanced):
- **Created ONLY after first successful login + profile completion**
- Links to AppUser via `AppUserId`
- Properties:
  - `Id` (Guid) - Primary key
  - `AppUserId` (Guid) - Foreign key to AppUser (for auth) - UNIQUE
  - `Email` (string, required, max 256, unique) - Copied from AppUser
  - `FirstName` (string, required, max 100) - **Set during profile completion**
  - `LastName` (string, required, max 100) - **Set during profile completion**
  - `DateOfBirth` (DateTime, required) - **Set during profile completion**
  - `PhoneNumber` (string, optional, max 20) - **Set during profile completion**
  - `ProfilePictureUrl` (string, optional, max 500)
  - `Bio` (string, optional, max 1000)
  - `Status` (UserStatus enum) - Active, Inactive, Suspended
  - `ProfileCompleted` (bool) - True after profile completion
  - `ProfileCompletedAt` (DateTime?) - Timestamp when profile was completed
  - `IsSubUser` (bool) - Computed: true if has any active master relations
  - `IsMasterUser` (bool) - Computed: true if has any active sub-user relations
  - NOTE: UserType enum is REMOVED - all users are equal, roles determined by relationships
  - `MasterUserRelations` (List<UserMasterSubUserRelation>) - Collection of master-sub relationships where this user is SubUser
  - `SubUserRelations` (List<UserMasterSubUserRelation>) - Collection of master-sub relationships where this user is MasterUser
  - `CreatedTasks` (List<Task>) - Navigation to tasks created by this user
  - `AssignedTasks` (List<TaskAssignment>) - Navigation to task assignments
  - `LastLoginAt` (DateTime?)
  - `CreatedAt` (DateTime) - When UserEntity was created (after profile completion)
  - `UpdatedAt` (DateTime?)

**UserMasterSubUserRelation** (Entity - Junction table with business logic):
- Represents Master-SubUser relationship (directional)
- **Recursive self-referencing relationship** on UserEntity
- Properties:
  - `Id` (Guid) - Primary key
  - `MasterUserId` (Guid) - Foreign key to UserEntity (inviter/master)
  - `SubUserId` (Guid) - Foreign key to UserEntity (invitee/sub)
  - `MasterUser` (UserEntity) - Navigation property (inviter)
  - `SubUser` (UserEntity) - Navigation property (invitee)
  - `InvitationToken` (string) - Unique token for invitation link
  - `InvitationEmail` (string, max 256) - Email address invitation was sent to
  - `InvitationStatus` (InvitationStatus enum) - Pending, Accepted, Rejected, Expired, Cancelled
  - `InvitedAt` (DateTime)
  - `AcceptedAt` (DateTime?)
  - `ExpiresAt` (DateTime) - Invitation expiration (7 days)
  - `Permissions` (List<string>) - Specific permissions SubUser has for MasterUser's tasks
  - `CanViewTasks` (bool) - SubUser can view MasterUser's tasks
  - `CanUpdateTasks` (bool) - SubUser can update MasterUser's task status
  - `CanCreateTasks` (bool) - SubUser can create tasks on behalf of MasterUser
  - `CanDeleteTasks` (bool) - SubUser can delete MasterUser's tasks
  - `IsActive` (bool) - Relationship is active
  - `DeactivatedAt` (DateTime?)
  - `DeactivatedBy` (Guid?) - Who deactivated (MasterUser or Admin)
  - `Notes` (string, optional, max 500) - Notes about relationship
  - `HierarchyDepth` (int) - Depth from root user (for queries/analytics)
  - `CreatedAt` (DateTime)
  - `UpdatedAt` (DateTime?)

#### Value Objects

**UserInvitationLink** (Value Object):
- `Token` (string) - Unique invitation token
- `MasterUserId` (Guid)
- `MasterUserName` (string)
- `InvitationUrl` (string) - Complete URL for invitation
- `ExpiresAt` (DateTime)
- `IsExpired` (bool) - Computed property

#### Enums

**InvitationStatus**:
```csharp
public enum InvitationStatus
{
    Pending = 1,      // Invitation sent, not yet accepted
    Accepted = 2,     // SubUser accepted invitation
    Rejected = 3,     // SubUser rejected invitation
    Expired = 4,      // Invitation expired
    Cancelled = 5     // MasterUser cancelled invitation
}
```

### Application Layer

#### DTOs (Data Transfer Objects)

**User Profile DTOs**:
```csharp
// Response DTOs
public record UserProfileDto(
    Guid Id,
    Guid AppUserId,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    DateTime DateOfBirth,
    int Age,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    string? Bio,
    UserStatus Status,
    bool ProfileCompleted,
    DateTime? ProfileCompletedAt,
    bool IsSubUser,
    bool IsMasterUser,
    int MasterUsersCount,
    int SubUsersCount,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public record UserListDto(
    Guid Id,
    string Email,
    string FullName,
    UserStatus Status,
    UserType UserType,
    bool IsMasterUser,
    bool IsSubUser,
    DateTime CreatedAt);

// Request DTOs (Registration - creates AppUser only)
public record RegisterRequest(
    string Email,
    string Username,
    string Password);

// Profile Completion (creates UserEntity after first login)
public record CompleteProfileRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string? PhoneNumber);

public record UpdateUserProfileRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string? PhoneNumber,
    string? Bio);

public record UploadProfilePictureRequest(
    string Base64Image);
```

**Master-SubUser Relationship DTOs**:
```csharp
// Request DTOs
public record CreateSubUserInvitationRequest(
    Guid MasterUserId,
    string SubUserEmail,
    bool CanViewTasks,
    bool CanUpdateTasks,
    bool CanCreateTasks,
    bool CanDeleteTasks,
    string? Notes);

public record AcceptSubUserInvitationRequest(
    string InvitationToken);

public record RejectSubUserInvitationRequest(
    string InvitationToken,
    string? Reason);

public record UpdateSubUserPermissionsRequest(
    Guid RelationId,
    bool CanViewTasks,
    bool CanUpdateTasks,
    bool CanCreateTasks,
    bool CanDeleteTasks);

public record RemoveSubUserRelationRequest(
    Guid RelationId,
    string? Reason);

// Response DTOs
public record SubUserInvitationDto(
    Guid Id,
    string InvitationToken,
    string InvitationUrl,
    Guid MasterUserId,
    string MasterUserName,
    string SubUserEmail,
    InvitationStatus Status,
    DateTime InvitedAt,
    DateTime ExpiresAt,
    bool IsExpired,
    DateTime? AcceptedAt);

public record MasterSubUserRelationDto(
    Guid Id,
    Guid MasterUserId,
    string MasterUserName,
    Guid SubUserId,
    string SubUserName,
    InvitationStatus InvitationStatus,
    bool CanViewTasks,
    bool CanUpdateTasks,
    bool CanCreateTasks,
    bool CanDeleteTasks,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? AcceptedAt);

public record SubUserDto(
    Guid Id,
    string FullName,
    string Email,
    InvitationStatus Status,
    bool CanViewTasks,
    bool CanUpdateTasks,
    bool CanCreateTasks,
    bool CanDeleteTasks,
    DateTime AddedAt);

public record MasterUserDto(
    Guid Id,
    string FullName,
    string Email,
    InvitationStatus Status,
    DateTime JoinedAt);
```

#### Commands (Write Operations)

**User Management Commands**:

1. **RegisterCommand** (Identity feature - creates AppUser only)
   - Creates AppUser with email, username, password
   - Sends email confirmation link
   - Does NOT create UserEntity yet
   - Returns registration success message

2. **CompleteProfileCommand** ⭐ NEW (creates UserEntity)
   - **Triggered on first successful login** (after email confirmation)
   - Validates AppUser exists and email confirmed
   - Validates UserEntity doesn't already exist for this AppUser
   - Creates UserEntity with profile data
   - Links UserEntity to AppUser via AppUserId
   - Sets ProfileCompleted = true, ProfileCompletedAt = now
   - Sets Status = Active
   - Checks for pending invitations by email
   - Auto-updates pending invitations with UserEntity.Id
   - Returns user profile DTO

3. **UpdateUserProfileCommand**
   - Validates UserEntity exists (profile completed)
   - Updates user profile information
   - Validates changes
   - Returns updated profile

4. **UploadProfilePictureCommand**
   - Validates UserEntity exists (profile completed)
   - Uploads and stores profile picture
   - Generates thumbnail
   - Updates UserEntity
   - Returns picture URL

5. **DeactivateUserCommand**
   - Deactivates UserEntity (if exists)
   - Deactivates AppUser (via Identity)
   - Revokes all tokens
   - Notifies MasterUsers if SubUser
   - Notifies SubUsers if MasterUser
   - Returns success

6. **ActivateUserCommand**
   - Activates user account (AppUser + UserEntity if exists)
   - Returns success

**Master-SubUser Relationship Commands**:

7. **CreateSubUserInvitationCommand**
   - Validates MasterUser exists
   - **CRITICAL**: Validates no cycle would be created:
     1. Check if SubUser → MasterUser already exists (direct reverse)
     2. Check if path exists SubUser → ... → MasterUser (indirect reverse)
     3. Use graph traversal (BFS/DFS) to detect reverse path
     4. Block if any reverse path found
   - Validates not inviting self (MasterUserId != SubUserId)
   - Validates no duplicate active relation
   - Generates unique invitation token
   - Creates UserMasterSubUserRelation
   - Generates invitation URL
   - Sends invitation email (with registration link if unregistered)
   - Returns invitation DTO

8. **ResendSubUserInvitationCommand**
   - Regenerates invitation token
   - Extends expiration
   - Sends new invitation email
   - Returns invitation DTO

9. **AcceptSubUserInvitationCommand**
   - Validates invitation token
   - Checks expiration
   - **Validates SubUser has completed profile** (UserEntity exists)
   - **Re-validates no cycle** (in case hierarchy changed since invitation sent):
     1. Check for reverse path SubUser → ... → MasterUser
     2. Block if cycle would be created
   - Ensures SubUser has UserEntity (profile completed)
   - Updates relation status to Accepted
   - Sets AcceptedAt timestamp
   - Sends confirmation to MasterUser
   - Returns relation DTO

10. **RejectSubUserInvitationCommand**
    - Validates invitation token
    - Updates status to Rejected
    - Notifies MasterUser
    - Returns success

11. **CancelSubUserInvitationCommand**
    - MasterUser cancels pending invitation
    - Updates status to Cancelled
    - Returns success

12. **UpdateSubUserPermissionsCommand**
    - Updates SubUser's permissions for MasterUser's tasks
    - Validates MasterUser owns relation
    - Returns updated relation DTO

13. **RemoveSubUserRelationCommand**
    - MasterUser removes SubUser relationship
    - Deactivates relation (soft delete)
    - Revokes SubUser's access to MasterUser's tasks
    - Sends notification to SubUser
    - Returns success

14. **LeaveAsMasterUserCommand**
    - SubUser leaves relationship with MasterUser
    - Deactivates relation
    - Sends notification to MasterUser
    - Returns success

#### Queries (Read Operations)

**User Queries**:

1. **GetUserProfileQuery**
   - Returns complete user profile
   - Includes master/sub user counts
   - Includes statistics

2. **GetMyProfileQuery**
   - Returns current authenticated user's profile
   - Includes full hierarchy information

3. **GetAllUsersQuery**
   - Returns paginated list of users
   - Supports filtering by UserType, Status
   - Supports search by name/email
   - Admin only

4. **GetUserByIdQuery**
   - Returns specific user by ID
   - Admin or related user only

5. **GetUserByEmailQuery**
   - Returns user by email
   - Admin only

**Master-SubUser Queries**:

6. **GetMyMasterUsersQuery**
   - Returns list of MasterUsers for current SubUser
   - Includes permissions and status

7. **GetMySubUsersQuery**
   - Returns list of SubUsers for current MasterUser
   - Includes permissions and status
   - Supports filtering by status

8. **GetUserMasterUsersQuery**
   - Admin: Get MasterUsers for any user
   - Returns list with permissions

9. **GetUserSubUsersQuery**
   - Admin: Get SubUsers for any user
   - Returns list with permissions

10. **GetPendingInvitationsQuery**
    - Returns pending invitations for current user (as MasterUser)
    - Includes expiration status

11. **GetMyPendingInvitationsQuery**
    - Returns pending invitations received by current user (as SubUser)
    - Includes invitation details

12. **GetSubUserRelationByIdQuery**
    - Returns specific relation details
    - MasterUser, SubUser, or Admin only

13. **GetSubUserPermissionsQuery**
    - Returns SubUser's permissions for specific MasterUser
    - Used for authorization checks

14. **CheckForCycleQuery** ⭐ NEW
    - Validates if creating relation (Master → Sub) would create cycle
    - Checks direct reverse: Sub → Master exists?
    - Checks indirect reverse: Path exists Sub → ... → Master?
    - Uses graph traversal (BFS/DFS) with max depth limit
    - Returns: WouldCreateCycle (bool), ExistingPath (List<Guid>)
    - Critical for CreateSubUserInvitationCommand validation

#### Validators

**FluentValidation validators for all commands**:

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Username)
            .NotEmpty()
            .Length(3, 50)
            .Matches("^[a-zA-Z0-9._-]+$");

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
            .GreaterThan(DateTime.Today.AddYears(-120))
            .WithMessage("Date of birth must be valid");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .Matches(@"^[\d\s\-\+\(\)]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}

public class CreateSubUserInvitationCommandValidator : AbstractValidator<CreateSubUserInvitationCommand>
{
    public CreateSubUserInvitationCommandValidator()
    {
        RuleFor(x => x.MasterUserId)
            .NotEmpty();

        RuleFor(x => x.SubUserEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Notes));

        // At least one permission must be granted
        RuleFor(x => x)
            .Must(x => x.CanViewTasks || x.CanUpdateTasks || x.CanCreateTasks || x.CanDeleteTasks)
            .WithMessage("At least one permission must be granted");
    }
}
```

### Infrastructure Layer

#### Repositories

**IUserRepository**:
```csharp
public interface IUserRepository
{
    // Basic CRUD
    Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserEntity?> GetByAppUserIdAsync(Guid appUserId, CancellationToken ct = default);
    Task<UserEntity?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<UserEntity>> GetAllAsync(CancellationToken ct = default);
    Task<PagedList<UserEntity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task AddAsync(UserEntity user, CancellationToken ct = default);
    void Update(UserEntity user);
    void Delete(UserEntity user);

    // Hierarchy queries
    Task<IReadOnlyList<UserEntity>> GetMasterUsersForSubUserAsync(Guid subUserId, CancellationToken ct = default);
    Task<IReadOnlyList<UserEntity>> GetSubUsersForMasterUserAsync(Guid masterUserId, CancellationToken ct = default);
    Task<IReadOnlyList<UserEntity>> GetUsersByTypeAsync(UserType userType, CancellationToken ct = default);

    // Search
    Task<IReadOnlyList<UserEntity>> SearchUsersAsync(string searchTerm, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
```

**IUserMasterSubUserRelationRepository**:
```csharp
public interface IUserMasterSubUserRelationRepository
{
    // Basic CRUD
    Task<UserMasterSubUserRelation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserMasterSubUserRelation?> GetByInvitationTokenAsync(string token, CancellationToken ct = default);
    Task<IReadOnlyList<UserMasterSubUserRelation>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(UserMasterSubUserRelation relation, CancellationToken ct = default);
    void Update(UserMasterSubUserRelation relation);
    void Delete(UserMasterSubUserRelation relation);

    // Relationship queries
    Task<IReadOnlyList<UserMasterSubUserRelation>> GetRelationsForMasterAsync(
        Guid masterUserId,
        CancellationToken ct = default);

    Task<IReadOnlyList<UserMasterSubUserRelation>> GetRelationsForSubUserAsync(
        Guid subUserId,
        CancellationToken ct = default);

    Task<UserMasterSubUserRelation?> GetRelationAsync(
        Guid masterUserId,
        Guid subUserId,
        CancellationToken ct = default);

    Task<bool> RelationExistsAsync(
        Guid masterUserId,
        Guid subUserId,
        CancellationToken ct = default);

    Task<IReadOnlyList<UserMasterSubUserRelation>> GetPendingInvitationsForMasterAsync(
        Guid masterUserId,
        CancellationToken ct = default);

    Task<IReadOnlyList<UserMasterSubUserRelation>> GetPendingInvitationsForSubUserEmailAsync(
        string email,
        CancellationToken ct = default);

    Task<IReadOnlyList<UserMasterSubUserRelation>> GetExpiredInvitationsAsync(
        CancellationToken ct = default);

    // Permission checks
    Task<bool> SubUserHasPermissionAsync(
        Guid subUserId,
        Guid masterUserId,
        string permission,
        CancellationToken ct = default);

    // ⭐ Cycle detection (CRITICAL for one-directional hierarchy)
    Task<bool> PathExistsAsync(
        Guid fromUserId,
        Guid toUserId,
        int maxDepth = 100,
        CancellationToken ct = default);

    Task<List<Guid>> FindPathAsync(
        Guid fromUserId,
        Guid toUserId,
        int maxDepth = 100,
        CancellationToken ct = default);

    Task<bool> WouldCreateCycleAsync(
        Guid proposedMasterUserId,
        Guid proposedSubUserId,
        CancellationToken ct = default);
}
```

#### EF Core Configuration

**UserEntityConfiguration**:
```csharp
public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppUserId)
            .IsRequired();

        builder.HasIndex(x => x.AppUserId)
            .IsUnique();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.DateOfBirth)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(x => x.ProfilePictureUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Bio)
            .HasMaxLength(1000);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.LastLoginAt);

        // Relationships
        builder.HasOne<AppUser>()
            .WithOne()
            .HasForeignKey<UserEntity>(x => x.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Master-SubUser relations (as Master)
        builder.HasMany(x => x.SubUserRelations)
            .WithOne(x => x.MasterUser)
            .HasForeignKey(x => x.MasterUserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade

        // Master-SubUser relations (as SubUser)
        builder.HasMany(x => x.MasterUserRelations)
            .WithOne(x => x.SubUser)
            .HasForeignKey(x => x.SubUserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent cascade

        // Ignore computed properties
        builder.Ignore(x => x.IsSubUser);
        builder.Ignore(x => x.IsMasterUser);
    }
}
```

**UserMasterSubUserRelationConfiguration**:
```csharp
public class UserMasterSubUserRelationConfiguration : IEntityTypeConfiguration<UserMasterSubUserRelation>
{
    public void Configure(EntityTypeBuilder<UserMasterSubUserRelation> builder)
    {
        builder.ToTable("UserMasterSubUserRelations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MasterUserId)
            .IsRequired();

        builder.Property(x => x.SubUserId)
            .IsRequired();

        builder.HasIndex(x => new { x.MasterUserId, x.SubUserId })
            .IsUnique();

        builder.Property(x => x.InvitationToken)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(x => x.InvitationToken)
            .IsUnique();

        builder.Property(x => x.InvitationStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.InvitedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.AcceptedAt);

        builder.Property(x => x.Permissions)
            .HasColumnName("Permissions")
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        builder.Property(x => x.CanViewTasks)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.CanUpdateTasks)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CanCreateTasks)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.CanDeleteTasks)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);

        builder.Property(x => x.DeactivatedAt);

        builder.Property(x => x.DeactivatedBy);

        // Relationships
        builder.HasOne(x => x.MasterUser)
            .WithMany(x => x.SubUserRelations)
            .HasForeignKey(x => x.MasterUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SubUser)
            .WithMany(x => x.MasterUserRelations)
            .HasForeignKey(x => x.SubUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### API Layer

#### Controllers

**UsersController**:
```csharp
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    public async Task<IActionResult> GetMyProfile();

    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileRequest request);

    [HttpPost("me/profile-picture")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UploadProfilePicture([FromBody] UploadProfilePictureRequest request);

    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(PagedList<UserListDto>), 200)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10);

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserById(Guid id);

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

**UserHierarchyController**:
```csharp
[ApiController]
[Route("api/v1/user-hierarchy")]
[Authorize]
public class UserHierarchyController : ControllerBase
{
    private readonly IMediator _mediator;

    // Master User operations
    [HttpPost("invitations")]
    [ProducesResponseType(typeof(SubUserInvitationDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateSubUserInvitation([FromBody] CreateSubUserInvitationRequest request);

    [HttpPost("invitations/{id:guid}/resend")]
    [ProducesResponseType(typeof(SubUserInvitationDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ResendInvitation(Guid id);

    [HttpPost("invitations/{id:guid}/cancel")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CancelInvitation(Guid id);

    [HttpGet("my-subusers")]
    [ProducesResponseType(typeof(List<SubUserDto>), 200)]
    public async Task<IActionResult> GetMySubUsers();

    [HttpGet("my-invitations/pending")]
    [ProducesResponseType(typeof(List<SubUserInvitationDto>), 200)]
    public async Task<IActionResult> GetMyPendingInvitations();

    // SubUser operations
    [HttpPost("invitations/accept")]
    [ProducesResponseType(typeof(MasterSubUserRelationDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptSubUserInvitationRequest request);

    [HttpPost("invitations/reject")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RejectInvitation([FromBody] RejectSubUserInvitationRequest request);

    [HttpGet("my-masters")]
    [ProducesResponseType(typeof(List<MasterUserDto>), 200)]
    public async Task<IActionResult> GetMyMasterUsers();

    [HttpPost("relations/{id:guid}/leave")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> LeaveMasterUser(Guid id);

    // Relationship management
    [HttpPut("relations/{id:guid}/permissions")]
    [ProducesResponseType(typeof(MasterSubUserRelationDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateSubUserPermissions(Guid id, [FromBody] UpdateSubUserPermissionsRequest request);

    [HttpDelete("relations/{id:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RemoveSubUserRelation(Guid id, [FromBody] RemoveSubUserRelationRequest request);

    [HttpGet("relations/{id:guid}")]
    [ProducesResponseType(typeof(MasterSubUserRelationDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetRelationById(Guid id);

    // Admin operations
    [HttpGet("users/{userId:guid}/masters")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(List<MasterUserDto>), 200)]
    public async Task<IActionResult> GetUserMasters(Guid userId);

    [HttpGet("users/{userId:guid}/subusers")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(List<SubUserDto>), 200)]
    public async Task<IActionResult> GetUserSubUsers(Guid userId);
}
```

## Business Rules

### User Management (Self-Registration)

1. **User Registration (Two-Stage Process)** ⭐:
   - ✅ **Stage 1: Registration** → Creates AppUser (Identity) only
     - User registers with email, username, password
     - System sends email confirmation link
     - AppUser created, UserEntity NOT created yet
   - ✅ **Stage 2: Email Confirmation**
     - User clicks confirmation link
     - AppUser.EmailConfirmed = true
     - User can now login
   - ✅ **Stage 3: First Login** → Mandatory profile completion
     - User logs in for first time
     - System checks: UserEntity exists for AppUser? NO → Redirect to profile completion
     - User CANNOT skip profile completion
   - ✅ **Stage 4: Profile Completion** → Creates UserEntity
     - User fills: FirstName, LastName, DateOfBirth, PhoneNumber (optional)
     - System creates UserEntity with AppUserId link
     - System checks for pending invitations by email
     - System auto-updates pending invitations with new UserEntity.Id
     - User now fully activated
   - ❌ NO admin-created accounts for regular users
   - Email must be unique across system
   - Username must be unique (handled by AppUser)
   - User must be at least 18 years old (validated at profile completion)
   - All users start as equals (no UserType distinction)

2. **User Status**:
   - Active: Can login and use system
   - Inactive: Cannot login, soft deleted
   - Suspended: Temporarily blocked by admin

3. **User Equality**:
   - ❌ NO UserType enum (removed)
   - All users are equal at registration
   - Roles (Master/Sub) determined by relationships only
   - Same user can be BOTH Master AND SubUser simultaneously
   - No restrictions on who can invite whom

### Master-SubUser Relationships (Recursive & Endless)

1. **Invitation Rules (Email-Based)**:
   - Only authenticated users can send invitations
   - Invitation sent to ANY email address (registered or not)
   - Invitation stored with `InvitationEmail` field
   - If email matches existing user → can accept immediately
   - If email has NO existing user → must register first with SAME email, then accept
   - Invitation token expires in 7 days
   - One pending invitation per Master-Sub email pair at a time
   - Can resend invitation (regenerates token, extends expiration)
   - MasterUser can cancel pending invitations
   - Invitee can reject invitations

2. **Relationship Rules (One-Directional N:M)**:
   - **Self-referencing N:M relationship** on UserEntity
   - One user can have UNLIMITED MasterUsers (people who invited them)
   - One user can have UNLIMITED SubUsers (people they invited)
   - ❌ **Cycles are BLOCKED**: A → B → C → A is INVALID (validation prevents)
   - ❌ **Bidirectional is BLOCKED**: If A → B exists, then B → A is INVALID
   - ✅ **Endless depth**: A → B → C → D → E → ... (no limit)
   - **Validation before creation**: Check for reverse path (B → ... → A) before allowing A → B
   - Cannot invite yourself (MasterUserId != SubUserId)
   - Cannot create duplicate active relations (same Master-Sub pair)
   - Both users must have Active status for relation to work
   - HierarchyDepth tracked for analytics (calculated with path traversal)

3. **Permission Rules**:
   - At least one permission must be granted
   - CanUpdateTasks implies CanViewTasks
   - CanDeleteTasks implies CanViewTasks
   - CanCreateTasks implies CanViewTasks
   - MasterUser can change permissions anytime
   - Permissions are relation-specific (different for each Master)

4. **Access Control**:
   - SubUser can view MasterUser's tasks if CanViewTasks = true
   - SubUser can update task status if CanUpdateTasks = true
   - SubUser can create tasks for MasterUser if CanCreateTasks = true
   - SubUser can delete MasterUser's tasks if CanDeleteTasks = true
   - SubUser cannot see other SubUsers of same MasterUser (unless explicitly shared)
   - MasterUser can always see SubUser's activity on their tasks

5. **Deactivation Rules**:
   - MasterUser can remove SubUser (soft delete relation)
   - SubUser can leave MasterUser relationship
   - Deactivating relation revokes all access immediately
   - Admin can deactivate any relation
   - Deactivating UserEntity deactivates all their relations

### Task Integration

1. **Task Creation**:
   - User creates task and is set as Creator
   - SubUser can create tasks on behalf of MasterUser if CanCreateTasks = true
   - Created tasks link to Creator's UserEntity

2. **Task Assignment**:
   - User can assign tasks to other users
   - SubUser can be assigned MasterUser's tasks
   - Multiple users can be assigned to same task
   - Assignment creates TaskAssignment record

3. **Task Access**:
   - User can view their created tasks
   - User can view tasks assigned to them
   - SubUser can view MasterUser's tasks if CanViewTasks = true
   - MasterUser can view SubUser's activity on their tasks

4. **Task Updates**:
   - Creator can always update their tasks
   - Assignee can update task status
   - SubUser can update MasterUser's task status if CanUpdateTasks = true

### Hierarchy Validation (Cycle Prevention)

1. **Pre-Creation Validation**:
   - Before creating relation (Master → Sub), system checks:
     1. Direct reverse: Sub → Master already exists? BLOCK
     2. Indirect reverse path: Sub → ... → Master exists? BLOCK
     3. Self-invitation: Master == Sub? BLOCK
     4. Duplicate: Master → Sub already exists? BLOCK
   - Use graph traversal (BFS/DFS) to detect paths
   - Maximum traversal depth: Configurable (e.g., 100 levels)

2. **One-Directional Hierarchy Rules**:
   - Tree/DAG structure enforced
   - No depth limit on hierarchy (but validation has max traversal depth)
   - ❌ Cycles BLOCKED (A → B → C → A validation fails)
   - ❌ Bidirectional BLOCKED (A → B AND B → A validation fails)
   - Each relationship is strictly directional (A invites B ≠ B invites A)
   - HierarchyDepth calculated from root for queries (no cycle handling needed)
   - No circular reference issues (structure guarantees acyclic)

3. **Admin Management**:
   - ℹ️ Admin users managed separately via **AdminUser entity** (future feature)
   - AdminUser will inherit/reference UserEntity
   - Admin operations on User hierarchy handled via AdminUser
   - User hierarchy is independent of admin roles

## Domain Events

### User Events
- `UserCreatedDomainEvent` - User account created
- `UserProfileUpdatedDomainEvent` - Profile information updated
- `UserProfilePictureUploadedDomainEvent` - Profile picture uploaded
- `UserDeactivatedDomainEvent` - User deactivated
- `UserActivatedDomainEvent` - User activated

### Hierarchy Events
- `SubUserInvitationCreatedDomainEvent` - Invitation sent
- `SubUserInvitationResentDomainEvent` - Invitation resent
- `SubUserInvitationAcceptedDomainEvent` - SubUser accepted invitation
- `SubUserInvitationRejectedDomainEvent` - SubUser rejected invitation
- `SubUserInvitationCancelledDomainEvent` - MasterUser cancelled invitation
- `SubUserInvitationExpiredDomainEvent` - Invitation expired
- `SubUserPermissionsUpdatedDomainEvent` - Permissions changed
- `SubUserRelationRemovedDomainEvent` - MasterUser removed SubUser
- `SubUserLeftMasterDomainEvent` - SubUser left relationship

## User Flows

### Flow 1: User Registration & Profile Completion (Two-Stage) ⭐

**Stage 1: Registration (Creates AppUser only)**:
1. New user visits public registration page
2. User fills registration form (email, username, password ONLY)
3. System validates email and username uniqueness
4. System creates AppUser (Identity layer) via `RegisterCommand`
5. **UserEntity NOT created yet**
6. System sends email confirmation link
7. User receives email

**Stage 2: Email Confirmation**:
8. User clicks confirmation link
9. System sets AppUser.EmailConfirmed = true
10. User redirected to login page

**Stage 3: First Login & Profile Completion**:
11. User logs in with username/password
12. System checks: Does UserEntity exist for this AppUser?
13. **UserEntity NOT found** → System redirects to profile completion page (MANDATORY)
14. User fills profile form:
    - FirstName (required)
    - LastName (required)
    - DateOfBirth (required, must be 18+)
    - PhoneNumber (optional)
15. User submits profile
16. System validates profile data
17. System creates UserEntity via `CompleteProfileCommand`
18. System links UserEntity to AppUser via AppUserId
19. System checks for pending invitations by AppUser.Email
20. System auto-updates pending invitations: Set SubUserId = UserEntity.Id
21. System notifies user of any pending invitations found
22. User now fully activated and redirected to dashboard
23. User can now accept pending invitations, create tasks, invite others

### Flow 2: Inviting Existing User (Profile Completed)

1. User A (registered, profile completed) invites User B via email via `CreateSubUserInvitationCommand`
2. System checks if email belongs to user with completed profile (UserEntity exists)
3. **Email HAS UserEntity** → System finds User B's UserEntity
4. System validates cycle prevention (no reverse path B → ... → A)
5. System creates UserMasterSubUserRelation (MasterUserId = A.Id, SubUserId = B.Id, Status = Pending, InvitationEmail = B's email)
6. System generates unique invitation token
7. System sends invitation email to User B with accept link
8. User B logs in and sees pending invitation
9. User B accepts via `AcceptSubUserInvitationCommand`
10. System re-validates cycle (in case hierarchy changed)
11. System updates relation status to Accepted, sets AcceptedAt
12. System sends confirmation to User A
13. User B can now access User A's tasks based on permissions

### Flow 3: Inviting User Without Profile (AppUser exists, UserEntity doesn't)

1. User A (profile completed) invites User C via email (user-c@example.com) via `CreateSubUserInvitationCommand`
2. System checks if email has UserEntity
3. **Email has AppUser but NO UserEntity** → User C registered but hasn't completed profile yet
4. System creates UserMasterSubUserRelation (MasterUserId = A.Id, SubUserId = NULL, Status = Pending, InvitationEmail = user-c@example.com)
5. System generates unique invitation token
6. System sends invitation email: "Complete your profile first, then accept invitation"
7. User C logs in → Redirected to profile completion (Flow 1, Stage 3)
8. User C completes profile → UserEntity created via `CompleteProfileCommand`
9. System finds pending invitations for User C's email
10. System auto-updates UserMasterSubUserRelation (sets SubUserId = C's UserEntity.Id)
11. System notifies User C of pending invitations
12. User C accepts invitation → Flow 2 (steps 9-13)

### Flow 4: Inviting Completely New User (No AppUser, No UserEntity)

1. User A (profile completed) invites User D via email (user-d@example.com) via `CreateSubUserInvitationCommand`
2. System checks if email has AppUser
3. **Email is NOT registered** → System creates pending invitation WITHOUT SubUserId
4. System creates UserMasterSubUserRelation (MasterUserId = A.Id, SubUserId = NULL, Status = Pending, InvitationEmail = user-d@example.com)
5. System generates unique invitation token
6. System sends invitation email with registration link: "Register first, complete profile, then accept invitation"
7. User D registers account with SAME email (user-d@example.com) → Flow 1, Stage 1
8. User D confirms email → Flow 1, Stage 2
9. User D first login → Redirected to profile completion → Flow 1, Stage 3
10. User D completes profile → UserEntity created via `CompleteProfileCommand`
11. System finds pending invitations for User D's email
12. System auto-updates UserMasterSubUserRelation (sets SubUserId = D's UserEntity.Id)
13. System notifies User D of pending invitations
14. User D accepts invitation → Flow 2 (steps 9-13)

### Flow 5: One-Directional Hierarchy with Validation

1. User A (profile completed) invites User B → Relation created (A is Master, B is Sub) ✅
2. User B (profile completed) invites User C → Relation created (B is Master, C is Sub) ✅
3. **Result so far**: A → B → C (valid one-directional chain)
4. User C tries to invite User A → **BLOCKED** ❌
   - System checks: Does path exist A → ... → C? YES (A → B → C)
   - Validation fails: Cannot create C → A (would create cycle)
   - Error: "Cannot invite user A. Creating this relation would form a cycle."
5. User B tries to invite User A → **BLOCKED** ❌
   - System checks: Does A → B already exist? YES
   - Validation fails: Cannot create bidirectional relation
   - Error: "Cannot invite user A. Reverse relation already exists (A → B)."
6. User D (new user, profile completed) invites User A → ✅ ALLOWED (no path exists A → ... → D)
7. **Final hierarchy**: D → A → B → C (valid one-directional tree)

### Flow 2: SubUser Views Master's Tasks

1. SubUser logs in
2. SubUser queries their MasterUsers via `GetMyMasterUsersQuery`
3. For each MasterUser, SubUser can view tasks if CanViewTasks = true
4. Task service checks permission via `GetSubUserPermissionsQuery`
5. If permission granted, tasks are displayed
6. SubUser can update task status if CanUpdateTasks = true

### Flow 3: Master User Updates SubUser Permissions

1. MasterUser views SubUsers via `GetMySubUsersQuery`
2. MasterUser selects SubUser to modify
3. MasterUser updates permissions via `UpdateSubUserPermissionsCommand`
4. System validates MasterUser owns relation
5. System updates permissions
6. System notifies SubUser of changes
7. New permissions take effect immediately

### Flow 4: SubUser Leaves Relationship

1. SubUser views MasterUsers via `GetMyMasterUsersQuery`
2. SubUser initiates leave via `LeaveAsMasterUserCommand`
3. System deactivates relation (IsActive = false)
4. System revokes SubUser's access to MasterUser's tasks
5. System notifies MasterUser
6. Relation preserved in DB for audit (soft delete)

### Flow 5: Admin Monitors Relationships

1. Admin views all users via `GetAllUsersQuery`
2. Admin selects specific user
3. Admin views user's MasterUsers via `GetUserMasterUsersQuery`
4. Admin views user's SubUsers via `GetUserSubUsersQuery`
5. Admin can deactivate any relation if needed
6. Admin can deactivate user accounts

## Security Considerations

1. **Authorization**:
   - All hierarchy endpoints require authentication
   - Permission checks on every task access
   - MasterUser can only modify their own SubUser relations
   - SubUser can only leave their own relations
   - Admin override for all operations

2. **Invitation Security**:
   - Cryptographically secure random tokens
   - Token expires in 7 days
   - One-time use (accepting invalidates token)
   - Cannot reuse cancelled/rejected tokens

3. **Data Privacy**:
   - SubUser cannot see other SubUsers of same MasterUser
   - MasterUser cannot see SubUser's other MasterUser relationships
   - Admin has full visibility (for moderation)

4. **Audit Trail**:
   - All relations track creation, acceptance, deactivation
   - Track who deactivated relation (MasterUser vs Admin vs SubUser)
   - Domain events for all relationship changes

## Implementation Notes

1. **UserEntity Enhancement**:
   - Add `AppUserId` foreign key (link to AppUser)
   - ❌ REMOVE `UserType` enum (not needed, all users equal)
   - Add navigation properties for relations (MasterUserRelations, SubUserRelations)
   - Add computed properties `IsSubUser`, `IsMasterUser` (based on relations count)
   - Self-referencing N:M relationship via UserMasterSubUserRelation

2. **New Entity (UserMasterSubUserRelation)**:
   - Create `UserMasterSubUserRelation` entity with all properties
   - **CRITICAL**: SubUserId can be NULL initially (for unregistered invitees)
   - Must store `InvitationEmail` to match pending invitations with new registrations
   - Implement business logic methods on entity
   - Add `HierarchyDepth` for analytics and cycle detection

3. **Repository Pattern**:
   - Implement both repositories (IUserRepository, IUserMasterSubUserRelationRepository)
   - Use eager loading for relations when needed
   - Optimize queries with includes
   - **CRITICAL**: Add query methods for finding pending invitations by email
   - Add recursive CTE queries for hierarchy traversal with cycle detection
   - Repository method: `GetPendingInvitationsByEmailAsync(string email)`

4. **Background Jobs**:
   - Job to expire old invitations (runs daily)
   - Job to cleanup inactive relations (optional)
   - Job to send reminder emails for pending invitations
   - **NEW**: Job to match pending invitations with new registrations (runs on user creation)
   - **NEW**: Trigger to check pending invitations after successful registration

5. **Testing**:
   - Unit tests for domain logic (recursive relations, cycle handling)
   - Integration tests for all API endpoints
   - Test permission enforcement
   - **Test invitation flows**: Existing user, new user, registration-then-accept
   - Test relationship deactivation
   - **Test recursive scenarios**: A→B→C→A cycles, bidirectional A↔B
   - Test hierarchy depth calculation with cycles
   - Test pending invitation matching after registration

## Status

**Domain Layer**:
- ✅ UserEntity exists (needs enhancement)
- ❌ UserMasterSubUserRelation (to be created)
- ❌ Enums: UserType, InvitationStatus (to be created)

**Application Layer**: ❌ To be generated

**Infrastructure Layer**: ❌ To be generated

**API Layer**: ❌ To be generated

**Tests**: ❌ To be generated
