# AdminUser Feature - Complete Specification

## 📋 Feature Overview

**Feature Name**: AdminUser
**Service**: User Service
**Purpose**: Production-ready administrative system with SuperAdmin capabilities, user management (block/unblock/suspend), complete task oversight (view all tasks, dismiss/suspend tasks), and role-based admin hierarchy

---

## 🎯 Core Requirements

### Admin Hierarchy
- **SuperAdmin**: Default admin created during system initialization
- **Admin**: Users promoted to admin by SuperAdmin
- SuperAdmin can promote any User to Admin
- SuperAdmin can demote any Admin back to User
- SuperAdmin CANNOT be demoted (system protection)
- Only ONE SuperAdmin exists in the system

### User Management Capabilities
- **Block User**: Prevent user login (cannot access system)
- **Unblock User**: Restore user access
- **Suspend User**: Temporary restriction (visible warning to user)
- **Activate User**: Remove suspension
- **View All Users**: Complete user list with filters
- **View User Details**: Full profile, tasks, activity history
- **Search Users**: By name, email, status

### Task Management Capabilities
- **View All Tasks**: Access to every task in the system
- **View Task Details**: Complete task information including comments, history
- **Dismiss Task**: Mark task as dismissed (soft delete, preserved for audit)
- **Suspend Task**: Temporarily freeze task (no updates allowed)
- **Unsuspend Task**: Restore task to active state
- **View Task Statistics**: System-wide task analytics
- **Search Tasks**: By creator, assignee, status, dates

### Audit & Monitoring
- **Activity Log**: Track all admin actions
- **User Activity**: Monitor user actions and patterns
- **Task Activity**: Monitor task creation and completion rates
- **System Statistics**: Dashboard with key metrics

---

## 🏗️ Domain Model

### 1. AdminUser Entity (Aggregate Root)

```csharp
public sealed class AdminUser : AggregateRoot<Guid>
{
    // Identity
    public Guid AppUserId { get; private set; }        // FK to AppUser (authentication)
    public Guid? UserEntityId { get; private set; }    // FK to UserEntity (profile) - nullable
    public string Email { get; private set; }          // Copied from AppUser
    public string Username { get; private set; }       // Copied from AppUser

    // Admin Properties
    public AdminRole AdminRole { get; private set; }   // SuperAdmin, Admin
    public AdminStatus AdminStatus { get; private set; } // Active, Inactive, Suspended
    public DateTime PromotedAt { get; private set; }   // When promoted to admin
    public Guid? PromotedBy { get; private set; }      // SuperAdmin who promoted (null for SuperAdmin)
    public string? PromotionReason { get; private set; } // Why promoted

    // Activity Tracking
    public DateTime? LastActiveAt { get; private set; }
    public int TotalActionsPerformed { get; private set; }
    public DateTime? LastActionAt { get; private set; }

    // Permissions
    public AdminPermissions Permissions { get; private set; } // Value object with permissions

    // Collections
    public List<AdminAction> Actions { get; private set; } // Audit trail

    // Factory Methods
    public static AdminUser CreateSuperAdmin(
        Guid appUserId,
        string email,
        string username)
    {
        var superAdmin = new AdminUser(Guid.NewGuid())
        {
            AppUserId = appUserId,
            UserEntityId = null, // SuperAdmin doesn't need profile
            Email = email,
            Username = username,
            AdminRole = AdminRole.SuperAdmin,
            AdminStatus = AdminStatus.Active,
            PromotedAt = DateTime.UtcNow,
            PromotedBy = null, // System created
            PromotionReason = "System Default SuperAdmin",
            TotalActionsPerformed = 0,
            Permissions = AdminPermissions.SuperAdminPermissions(),
            Actions = new List<AdminAction>()
        };

        superAdmin.RaiseDomainEvent(new SuperAdminCreatedDomainEvent(superAdmin.Id));
        return superAdmin;
    }

    public static AdminUser PromoteUserToAdmin(
        Guid appUserId,
        Guid userEntityId,
        string email,
        string username,
        Guid promotedBySuperAdminId,
        string reason)
    {
        var admin = new AdminUser(Guid.NewGuid())
        {
            AppUserId = appUserId,
            UserEntityId = userEntityId,
            Email = email,
            Username = username,
            AdminRole = AdminRole.Admin,
            AdminStatus = AdminStatus.Active,
            PromotedAt = DateTime.UtcNow,
            PromotedBy = promotedBySuperAdminId,
            PromotionReason = reason,
            TotalActionsPerformed = 0,
            Permissions = AdminPermissions.StandardAdminPermissions(),
            Actions = new List<AdminAction>()
        };

        admin.RaiseDomainEvent(new UserPromotedToAdminDomainEvent(
            admin.Id, appUserId, promotedBySuperAdminId));
        return admin;
    }

    // Domain Methods
    public void RecordAction(AdminActionType actionType, string targetId, string details)
    {
        var action = AdminAction.Create(Id, actionType, targetId, details);
        Actions.Add(action);
        TotalActionsPerformed++;
        LastActionAt = DateTime.UtcNow;
        LastActiveAt = DateTime.UtcNow;
    }

    public void Suspend(Guid suspendedBy, string reason)
    {
        if (AdminRole == AdminRole.SuperAdmin)
            throw new AdminInvalidOperationException("Cannot suspend SuperAdmin");

        AdminStatus = AdminStatus.Suspended;
        RecordAction(AdminActionType.AdminSuspended, Id.ToString(),
            $"Suspended by {suspendedBy}. Reason: {reason}");
        RaiseDomainEvent(new AdminSuspendedDomainEvent(Id, suspendedBy, reason));
    }

    public void Activate()
    {
        AdminStatus = AdminStatus.Active;
        RecordAction(AdminActionType.AdminActivated, Id.ToString(), "Admin activated");
        RaiseDomainEvent(new AdminActivatedDomainEvent(Id));
    }

    public void DemoteToUser(Guid demotedBy)
    {
        if (AdminRole == AdminRole.SuperAdmin)
            throw new AdminInvalidOperationException("Cannot demote SuperAdmin");

        AdminStatus = AdminStatus.Inactive;
        RecordAction(AdminActionType.AdminDemoted, Id.ToString(),
            $"Demoted to User by {demotedBy}");
        RaiseDomainEvent(new AdminDemotedToUserDomainEvent(Id, AppUserId, demotedBy));
    }

    public bool CanPerformAction(AdminPermissionType permissionType)
    {
        if (AdminStatus != AdminStatus.Active)
            return false;

        if (AdminRole == AdminRole.SuperAdmin)
            return true; // SuperAdmin has all permissions

        return Permissions.HasPermission(permissionType);
    }
}
```

### 2. AdminPermissions (Value Object)

```csharp
public sealed class AdminPermissions
{
    public bool CanBlockUsers { get; private set; }
    public bool CanUnblockUsers { get; private set; }
    public bool CanSuspendUsers { get; private set; }
    public bool CanViewAllUsers { get; private set; }
    public bool CanViewAllTasks { get; private set; }
    public bool CanDismissTasks { get; private set; }
    public bool CanSuspendTasks { get; private set; }
    public bool CanViewSystemStats { get; private set; }
    public bool CanPromoteToAdmin { get; private set; }      // SuperAdmin only
    public bool CanDemoteAdmin { get; private set; }         // SuperAdmin only
    public bool CanManageAdmins { get; private set; }        // SuperAdmin only

    public static AdminPermissions SuperAdminPermissions()
    {
        return new AdminPermissions
        {
            CanBlockUsers = true,
            CanUnblockUsers = true,
            CanSuspendUsers = true,
            CanViewAllUsers = true,
            CanViewAllTasks = true,
            CanDismissTasks = true,
            CanSuspendTasks = true,
            CanViewSystemStats = true,
            CanPromoteToAdmin = true,
            CanDemoteAdmin = true,
            CanManageAdmins = true
        };
    }

    public static AdminPermissions StandardAdminPermissions()
    {
        return new AdminPermissions
        {
            CanBlockUsers = true,
            CanUnblockUsers = true,
            CanSuspendUsers = true,
            CanViewAllUsers = true,
            CanViewAllTasks = true,
            CanDismissTasks = true,
            CanSuspendTasks = true,
            CanViewSystemStats = true,
            CanPromoteToAdmin = false,  // Admin cannot promote
            CanDemoteAdmin = false,     // Admin cannot demote
            CanManageAdmins = false     // Admin cannot manage other admins
        };
    }

    public bool HasPermission(AdminPermissionType permissionType)
    {
        return permissionType switch
        {
            AdminPermissionType.BlockUsers => CanBlockUsers,
            AdminPermissionType.UnblockUsers => CanUnblockUsers,
            AdminPermissionType.SuspendUsers => CanSuspendUsers,
            AdminPermissionType.ViewAllUsers => CanViewAllUsers,
            AdminPermissionType.ViewAllTasks => CanViewAllTasks,
            AdminPermissionType.DismissTasks => CanDismissTasks,
            AdminPermissionType.SuspendTasks => CanSuspendTasks,
            AdminPermissionType.ViewSystemStats => CanViewSystemStats,
            AdminPermissionType.PromoteToAdmin => CanPromoteToAdmin,
            AdminPermissionType.DemoteAdmin => CanDemoteAdmin,
            AdminPermissionType.ManageAdmins => CanManageAdmins,
            _ => false
        };
    }
}
```

### 3. AdminAction Entity

```csharp
public sealed class AdminAction : Entity<Guid>
{
    public Guid AdminUserId { get; private set; }
    public AdminActionType ActionType { get; private set; }
    public string TargetId { get; private set; }        // User ID or Task ID
    public string TargetType { get; private set; }      // "User" or "Task"
    public string Details { get; private set; }         // JSON or description
    public DateTime PerformedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    public static AdminAction Create(
        Guid adminUserId,
        AdminActionType actionType,
        string targetId,
        string details)
    {
        return new AdminAction(Guid.NewGuid())
        {
            AdminUserId = adminUserId,
            ActionType = actionType,
            TargetId = targetId,
            TargetType = DetermineTargetType(actionType),
            Details = details,
            PerformedAt = DateTime.UtcNow
        };
    }

    private static string DetermineTargetType(AdminActionType actionType)
    {
        return actionType switch
        {
            AdminActionType.UserBlocked => "User",
            AdminActionType.UserUnblocked => "User",
            AdminActionType.UserSuspended => "User",
            AdminActionType.UserActivated => "User",
            AdminActionType.TaskDismissed => "Task",
            AdminActionType.TaskSuspended => "Task",
            AdminActionType.TaskUnsuspended => "Task",
            AdminActionType.UserPromotedToAdmin => "User",
            AdminActionType.AdminDemoted => "Admin",
            _ => "System"
        };
    }
}
```

---

## 🔢 Enumerations

### AdminRole
```csharp
public enum AdminRole
{
    SuperAdmin = 1,
    Admin = 2
}
```

### AdminStatus
```csharp
public enum AdminStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}
```

### AdminActionType
```csharp
public enum AdminActionType
{
    // User Management
    UserBlocked = 1,
    UserUnblocked = 2,
    UserSuspended = 3,
    UserActivated = 4,
    UserViewed = 5,

    // Task Management
    TaskViewed = 10,
    TaskDismissed = 11,
    TaskSuspended = 12,
    TaskUnsuspended = 13,

    // Admin Management
    UserPromotedToAdmin = 20,
    AdminDemoted = 21,
    AdminSuspended = 22,
    AdminActivated = 23,

    // System
    SystemStatsViewed = 30,
    AuditLogViewed = 31
}
```

### AdminPermissionType
```csharp
public enum AdminPermissionType
{
    BlockUsers = 1,
    UnblockUsers = 2,
    SuspendUsers = 3,
    ViewAllUsers = 4,
    ViewAllTasks = 5,
    DismissTasks = 6,
    SuspendTasks = 7,
    ViewSystemStats = 8,
    PromoteToAdmin = 9,
    DemoteAdmin = 10,
    ManageAdmins = 11
}
```

---

## 📊 DTOs

### AdminUserDto
```csharp
public sealed record AdminUserDto(
    Guid Id,
    Guid AppUserId,
    Guid? UserEntityId,
    string Email,
    string Username,
    AdminRole AdminRole,
    AdminStatus AdminStatus,
    DateTime PromotedAt,
    Guid? PromotedBy,
    string? PromotionReason,
    DateTime? LastActiveAt,
    int TotalActionsPerformed,
    AdminPermissionsDto Permissions,
    DateTime CreatedAt
);
```

### AdminActionDto
```csharp
public sealed record AdminActionDto(
    Guid Id,
    Guid AdminUserId,
    string AdminUsername,
    AdminActionType ActionType,
    string TargetId,
    string TargetType,
    string Details,
    DateTime PerformedAt,
    string? IpAddress
);
```

### SystemStatisticsDto
```csharp
public sealed record SystemStatisticsDto(
    int TotalUsers,
    int ActiveUsers,
    int BlockedUsers,
    int SuspendedUsers,
    int TotalTasks,
    int CompletedTasks,
    int ActiveTasks,
    int DismissedTasks,
    int SuspendedTasks,
    int TotalAdmins,
    DateTime GeneratedAt
);
```

---

## 💼 Application Layer

### Commands

#### 1. PromoteUserToAdminCommand
```csharp
public sealed record PromoteUserToAdminCommand(
    Guid UserId,
    string Reason
) : IRequest<Result<Guid>>;

// Handler validates:
// - Requester is SuperAdmin
// - User exists and has completed profile
// - User is not already an admin
// - Creates AdminUser entity
```

#### 2. DemoteAdminToUserCommand
```csharp
public sealed record DemoteAdminToUserCommand(
    Guid AdminUserId
) : IRequest<Result>;

// Handler validates:
// - Requester is SuperAdmin
// - Target is not SuperAdmin
// - Marks AdminUser as Inactive
```

#### 3. BlockUserCommand
```csharp
public sealed record BlockUserCommand(
    Guid UserId,
    string Reason
) : IRequest<Result>;

// Handler:
// - Updates AppUser.Status = Locked
// - Records admin action
// - Revokes all active sessions
```

#### 4. UnblockUserCommand
```csharp
public sealed record UnblockUserCommand(
    Guid UserId
) : IRequest<Result>;

// Handler:
// - Updates AppUser.Status = Active
// - Records admin action
```

#### 5. SuspendUserCommand
```csharp
public sealed record SuspendUserCommand(
    Guid UserId,
    string Reason,
    DateTime? SuspendUntil
) : IRequest<Result>;

// Handler:
// - Updates AppUser.Status = Suspended
// - Sets suspension expiry
// - Records admin action
```

#### 6. DismissTaskCommand
```csharp
public sealed record DismissTaskCommand(
    Guid TaskId,
    string Reason
) : IRequest<Result>;

// Handler:
// - Updates Task.Status = Dismissed (or IsActive = false)
// - Preserves task for audit
// - Records in TaskHistory
// - Records admin action
```

#### 7. SuspendTaskCommand
```csharp
public sealed record SuspendTaskCommand(
    Guid TaskId,
    string Reason
) : IRequest<Result>;

// Handler:
// - Updates Task.Status = Suspended
// - Prevents any updates
// - Records admin action
```

#### 8. UnsuspendTaskCommand
```csharp
public sealed record UnsuspendTaskCommand(
    Guid TaskId
) : IRequest<Result>;

// Handler:
// - Restores previous status (from TaskHistory)
// - Allows updates again
// - Records admin action
```

### Queries

#### 1. GetAllAdminsQuery
```csharp
public sealed record GetAllAdminsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    AdminRole? Role = null,
    AdminStatus? Status = null
) : IRequest<Result<PagedResult<AdminUserDto>>>;
```

#### 2. GetAdminByIdQuery
```csharp
public sealed record GetAdminByIdQuery(
    Guid AdminUserId
) : IRequest<Result<AdminUserDto>>;
```

#### 3. GetAllUsersQuery (Admin Version)
```csharp
public sealed record GetAllUsersQuery(
    int PageNumber = 1,
    int PageSize = 20,
    UserStatus? Status = null,
    string? SearchTerm = null
) : IRequest<Result<PagedResult<UserDto>>>;
```

#### 4. GetAllTasksQuery (Admin Version)
```csharp
public sealed record GetAllTasksQuery(
    int PageNumber = 1,
    int PageSize = 20,
    TaskStatus? Status = null,
    Guid? CreatedBy = null,
    Guid? AssignedTo = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
) : IRequest<Result<PagedResult<TaskDto>>>;
```

#### 5. GetAdminActionsQuery
```csharp
public sealed record GetAdminActionsQuery(
    Guid? AdminUserId = null,
    AdminActionType? ActionType = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PagedResult<AdminActionDto>>>;
```

#### 6. GetSystemStatisticsQuery
```csharp
public sealed record GetSystemStatisticsQuery()
    : IRequest<Result<SystemStatisticsDto>>;
```

#### 7. CheckIsSuperAdminQuery
```csharp
public sealed record CheckIsSuperAdminQuery(
    Guid UserId
) : IRequest<Result<bool>>;
```

---

## 🔒 Business Rules

### SuperAdmin Rules
1. Only ONE SuperAdmin exists in the system
2. SuperAdmin is created during system initialization
3. SuperAdmin CANNOT be demoted
4. SuperAdmin CANNOT be suspended
5. SuperAdmin CANNOT be blocked
6. SuperAdmin has ALL permissions
7. SuperAdmin can promote any User to Admin
8. SuperAdmin can demote any Admin to User
9. SuperAdmin can suspend/activate Admins

### Admin Rules
10. Admin users are promoted by SuperAdmin only
11. Admin can block/unblock regular users
12. Admin can suspend/activate regular users
13. Admin can view all users in the system
14. Admin can view all tasks in the system
15. Admin can dismiss tasks (soft delete with audit)
16. Admin can suspend/unsuspend tasks
17. Admin CANNOT promote users to Admin
18. Admin CANNOT demote other Admins
19. Admin CANNOT suspend/block other Admins
20. Admin can be demoted by SuperAdmin
21. Admin can be suspended by SuperAdmin
22. Suspended Admin cannot perform any actions

### User Management Rules
23. Blocking user revokes all active sessions immediately
24. Blocked user cannot login
25. Suspended user can login but sees warning
26. Suspended user has limited access
27. All admin actions recorded in audit trail
28. Block/Unblock requires reason
29. Suspension can have expiry date (auto-restore)

### Task Management Rules
30. Dismissed tasks are soft-deleted (preserved for audit)
31. Dismissed tasks appear in audit reports only
32. Suspended tasks cannot be updated by anyone (except unsuspend)
33. Suspended tasks remain assigned
34. Unsuspending task restores previous status
35. All task admin actions recorded in TaskHistory
36. Task dismissal/suspension requires reason

### Audit Rules
37. All admin actions tracked with timestamp
38. Admin actions include IP address and user agent
39. Target ID and target type recorded for each action
40. SuperAdmin actions never expire from audit log
41. Admin actions retained for compliance (7 years minimum)

---

## 🎬 User Flows

### Flow 1: System Initialization - Create SuperAdmin

**Scenario**: System starts for the first time, SuperAdmin is created

**Steps**:
1. System checks if SuperAdmin exists
2. If not exists, create default SuperAdmin:
   - Email: superadmin@taskflow.com (from config)
   - Username: superadmin
   - Password: Generated secure password
3. Create AppUser with SuperAdmin role
4. Create AdminUser with AdminRole.SuperAdmin
5. Send credentials to system owner
6. SuperAdminCreatedDomainEvent raised
7. Audit log entry created

**Validation**:
- Only runs once (check AdminUser table for SuperAdmin)
- Credentials securely generated and delivered

---

### Flow 2: SuperAdmin Promotes User to Admin

**Scenario**: SuperAdmin wants to promote a user to Admin

**Actor**: SuperAdmin

**Preconditions**:
- User exists with completed profile
- User is not already an Admin
- SuperAdmin is authenticated

**Steps**:
1. SuperAdmin navigates to User Management
2. Searches for user by email/username
3. Clicks "Promote to Admin"
4. Enters promotion reason (required)
5. System validates:
   - Requester is SuperAdmin
   - User exists and has completed profile
   - User is not already Admin
6. System creates AdminUser entity:
   - Links to AppUser and UserEntity
   - Sets AdminRole = Admin
   - Sets AdminStatus = Active
   - Records PromotedBy = SuperAdmin.Id
   - Permissions = StandardAdminPermissions
7. System updates AppUser.Roles (adds "Admin")
8. System raises UserPromotedToAdminDomainEvent
9. System sends email notification to user
10. Admin action recorded in audit trail
11. Success message displayed

**Result**:
- User is now Admin
- User can access Admin Panel
- Audit trail updated

---

### Flow 3: Admin Blocks User

**Scenario**: Admin blocks a user for policy violation

**Actor**: Admin or SuperAdmin

**Preconditions**:
- Admin is authenticated
- Target user exists and is not Admin

**Steps**:
1. Admin navigates to User Management
2. Finds user to block
3. Clicks "Block User"
4. Enters block reason (required)
5. System validates:
   - Requester is Admin or SuperAdmin
   - Target is not Admin or SuperAdmin
   - Requester has CanBlockUsers permission
6. System updates AppUser:
   - Status = Locked
   - LockoutEndAt = null (permanent until unblocked)
7. System revokes all active refresh tokens
8. System raises UserBlockedDomainEvent
9. Admin action recorded
10. User receives email notification
11. Active sessions terminated

**Result**:
- User cannot login
- All sessions terminated
- Audit trail updated

---

### Flow 4: Admin Views All Tasks

**Scenario**: Admin wants to see all tasks in the system

**Actor**: Admin or SuperAdmin

**Preconditions**:
- Admin is authenticated
- Admin has CanViewAllTasks permission

**Steps**:
1. Admin navigates to Task Management
2. System displays task list with filters:
   - Status (Draft, Assigned, InProgress, etc.)
   - Priority (Low, Medium, High, Urgent)
   - Creator
   - Assignee
   - Date range
3. Admin applies filters
4. System queries all tasks (ignoring Master-SubUser permissions)
5. System returns paginated results
6. Admin can click task to view details:
   - Task information
   - Assignment history
   - Comments
   - TaskHistory
7. Admin action recorded (TaskViewed)

**Result**:
- Admin sees complete task overview
- Can monitor all system activity

---

### Flow 5: Admin Dismisses Task

**Scenario**: Admin dismisses inappropriate task

**Actor**: Admin or SuperAdmin

**Preconditions**:
- Admin is authenticated
- Admin has CanDismissTasks permission
- Task exists

**Steps**:
1. Admin views task details
2. Clicks "Dismiss Task"
3. Enters dismissal reason (required)
4. System validates:
   - Requester is Admin/SuperAdmin
   - Requester has CanDismissTasks permission
5. System updates Task:
   - Status = Dismissed (or IsActive = false)
   - Task preserved for audit
6. System adds TaskHistory entry:
   - Action = Dismissed
   - PerformedBy = Admin.Id
   - Details = Reason
7. System raises TaskDismissedDomainEvent
8. Admin action recorded
9. Creator and assignee notified
10. Task removed from active lists

**Result**:
- Task no longer visible to users
- Task preserved in audit reports
- Notifications sent

---

### Flow 6: Admin Suspends Task

**Scenario**: Admin temporarily suspends task under investigation

**Actor**: Admin or SuperAdmin

**Preconditions**:
- Admin is authenticated
- Admin has CanSuspendTasks permission
- Task exists and is not already suspended

**Steps**:
1. Admin views task
2. Clicks "Suspend Task"
3. Enters suspension reason
4. System validates permissions
5. System updates Task:
   - Status = Suspended
   - No updates allowed (except unsuspend)
6. System records in TaskHistory
7. Admin action recorded
8. Users notified

**Result**:
- Task frozen (no updates)
- Visible but locked
- Can be unsuspended later

---

### Flow 7: SuperAdmin Demotes Admin to User

**Scenario**: SuperAdmin removes admin privileges

**Actor**: SuperAdmin

**Preconditions**:
- SuperAdmin is authenticated
- Target is Admin (not SuperAdmin)

**Steps**:
1. SuperAdmin navigates to Admin Management
2. Selects admin to demote
3. Clicks "Demote to User"
4. Confirms action
5. System validates:
   - Requester is SuperAdmin
   - Target is not SuperAdmin
6. System updates AdminUser:
   - AdminStatus = Inactive
7. System updates AppUser:
   - Removes "Admin" from Roles
8. System raises AdminDemotedToUserDomainEvent
9. Admin action recorded
10. User notified
11. Admin loses access to Admin Panel

**Result**:
- User reverted to regular user
- Admin privileges removed
- Audit trail preserved

---

## 🗄️ Infrastructure Layer

### Repository Interface

```csharp
public interface IAdminUserRepository
{
    // Basic CRUD
    Task<AdminUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdminUser?> GetByAppUserIdAsync(Guid appUserId, CancellationToken ct = default);
    Task<AdminUser?> GetSuperAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AdminUser>> GetAllAsync(
        AdminRole? role = null,
        AdminStatus? status = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken ct = default);
    Task AddAsync(AdminUser adminUser, CancellationToken ct = default);
    void Update(AdminUser adminUser);

    // Checks
    Task<bool> SuperAdminExistsAsync(CancellationToken ct = default);
    Task<bool> IsUserAdminAsync(Guid appUserId, CancellationToken ct = default);
    Task<bool> IsSuperAdminAsync(Guid appUserId, CancellationToken ct = default);

    // Admin Actions
    Task<IReadOnlyList<AdminAction>> GetAdminActionsAsync(
        Guid? adminUserId = null,
        AdminActionType? actionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken ct = default);

    // Statistics
    Task<int> GetTotalAdminsCountAsync(CancellationToken ct = default);
    Task<int> GetActiveAdminsCountAsync(CancellationToken ct = default);
}
```

### EF Core Configuration

```csharp
public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("AdminUsers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AppUserId).IsRequired();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AdminRole).IsRequired();
        builder.Property(x => x.AdminStatus).IsRequired();
        builder.Property(x => x.PromotedAt).IsRequired();
        builder.Property(x => x.PromotionReason).HasMaxLength(500);
        builder.Property(x => x.TotalActionsPerformed).HasDefaultValue(0);

        // Value Object
        builder.OwnsOne(x => x.Permissions, p =>
        {
            p.Property(x => x.CanBlockUsers).IsRequired();
            p.Property(x => x.CanUnblockUsers).IsRequired();
            p.Property(x => x.CanSuspendUsers).IsRequired();
            p.Property(x => x.CanViewAllUsers).IsRequired();
            p.Property(x => x.CanViewAllTasks).IsRequired();
            p.Property(x => x.CanDismissTasks).IsRequired();
            p.Property(x => x.CanSuspendTasks).IsRequired();
            p.Property(x => x.CanViewSystemStats).IsRequired();
            p.Property(x => x.CanPromoteToAdmin).IsRequired();
            p.Property(x => x.CanDemoteAdmin).IsRequired();
            p.Property(x => x.CanManageAdmins).IsRequired();
        });

        // Owned Collection
        builder.OwnsMany(x => x.Actions, a =>
        {
            a.ToTable("AdminActions");
            a.WithOwner().HasForeignKey("AdminUserId");
            a.HasKey(x => x.Id);
            a.Property(x => x.ActionType).IsRequired();
            a.Property(x => x.TargetId).IsRequired().HasMaxLength(50);
            a.Property(x => x.TargetType).IsRequired().HasMaxLength(20);
            a.Property(x => x.Details).IsRequired().HasMaxLength(2000);
            a.Property(x => x.PerformedAt).IsRequired();
            a.Property(x => x.IpAddress).HasMaxLength(50);
            a.Property(x => x.UserAgent).HasMaxLength(500);

            a.HasIndex(x => x.PerformedAt);
            a.HasIndex(x => x.ActionType);
        });

        // Indexes
        builder.HasIndex(x => x.AppUserId).IsUnique();
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => x.AdminRole);
        builder.HasIndex(x => x.AdminStatus);

        // Base entity configuration
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
```

---

## 🌐 API Layer

### AdminController

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    // Admin Management (SuperAdmin only)
    [HttpPost("promote")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PromoteToAdmin(
        [FromBody] PromoteUserToAdminRequest request)
    {
        var command = request.Adapt<PromoteUserToAdminCommand>();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("demote/{adminUserId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DemoteToUser(Guid adminUserId)
    {
        var command = new DemoteAdminToUserCommand(adminUserId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllAdmins(
        [FromQuery] GetAllAdminsRequest request)
    {
        var query = request.Adapt<GetAllAdminsQuery>();
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // User Management
    [HttpPost("users/{userId}/block")]
    public async Task<IActionResult> BlockUser(
        Guid userId,
        [FromBody] BlockUserRequest request)
    {
        var command = new BlockUserCommand(userId, request.Reason);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("users/{userId}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid userId)
    {
        var command = new UnblockUserCommand(userId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("users/{userId}/suspend")]
    public async Task<IActionResult> SuspendUser(
        Guid userId,
        [FromBody] SuspendUserRequest request)
    {
        var command = request.Adapt<SuspendUserCommand>();
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] GetAllUsersRequest request)
    {
        var query = request.Adapt<GetAllUsersQuery>();
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // Task Management
    [HttpGet("tasks")]
    public async Task<IActionResult> GetAllTasks(
        [FromQuery] GetAllTasksRequest request)
    {
        var query = request.Adapt<GetAllTasksQuery>();
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("tasks/{taskId}/dismiss")]
    public async Task<IActionResult> DismissTask(
        Guid taskId,
        [FromBody] DismissTaskRequest request)
    {
        var command = new DismissTaskCommand(taskId, request.Reason);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("tasks/{taskId}/suspend")]
    public async Task<IActionResult> SuspendTask(
        Guid taskId,
        [FromBody] SuspendTaskRequest request)
    {
        var command = new SuspendTaskCommand(taskId, request.Reason);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    [HttpPost("tasks/{taskId}/unsuspend")]
    public async Task<IActionResult> UnsuspendTask(Guid taskId)
    {
        var command = new UnsuspendTaskCommand(taskId);
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }

    // Audit & Statistics
    [HttpGet("actions")]
    public async Task<IActionResult> GetAdminActions(
        [FromQuery] GetAdminActionsRequest request)
    {
        var query = request.Adapt<GetAdminActionsQuery>();
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetSystemStatistics()
    {
        var query = new GetSystemStatisticsQuery();
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
```

---

## 🎯 Domain Events

```csharp
public sealed record SuperAdminCreatedDomainEvent(Guid AdminUserId) : IDomainEvent;

public sealed record UserPromotedToAdminDomainEvent(
    Guid AdminUserId,
    Guid AppUserId,
    Guid PromotedBy) : IDomainEvent;

public sealed record AdminDemotedToUserDomainEvent(
    Guid AdminUserId,
    Guid AppUserId,
    Guid DemotedBy) : IDomainEvent;

public sealed record AdminSuspendedDomainEvent(
    Guid AdminUserId,
    Guid SuspendedBy,
    string Reason) : IDomainEvent;

public sealed record AdminActivatedDomainEvent(Guid AdminUserId) : IDomainEvent;

public sealed record UserBlockedDomainEvent(
    Guid UserId,
    Guid BlockedBy,
    string Reason) : IDomainEvent;

public sealed record UserUnblockedDomainEvent(
    Guid UserId,
    Guid UnblockedBy) : IDomainEvent;

public sealed record UserSuspendedDomainEvent(
    Guid UserId,
    Guid SuspendedBy,
    string Reason,
    DateTime? SuspendUntil) : IDomainEvent;

public sealed record TaskDismissedDomainEvent(
    Guid TaskId,
    Guid DismissedBy,
    string Reason) : IDomainEvent;

public sealed record TaskSuspendedDomainEvent(
    Guid TaskId,
    Guid SuspendedBy,
    string Reason) : IDomainEvent;

public sealed record TaskUnsuspendedDomainEvent(
    Guid TaskId,
    Guid UnsuspendedBy) : IDomainEvent;
```

---

## 🚨 Domain Exceptions

```csharp
public sealed class AdminUserNotFoundException : NotFoundException
{
    public AdminUserNotFoundException(Guid adminUserId)
        : base($"AdminUser with ID {adminUserId} was not found") { }
}

public sealed class AdminInvalidOperationException : DomainException
{
    public AdminInvalidOperationException(string message)
        : base(message) { }
}

public sealed class SuperAdminCannotBeModifiedException : DomainException
{
    public SuperAdminCannotBeModifiedException()
        : base("SuperAdmin cannot be demoted, suspended, or blocked") { }
}

public sealed class InsufficientAdminPermissionsException : DomainException
{
    public InsufficientAdminPermissionsException(AdminPermissionType permission)
        : base($"Insufficient permissions: {permission} required") { }
}

public sealed class AdminAlreadyExistsException : DomainException
{
    public AdminAlreadyExistsException(Guid userId)
        : base($"User {userId} is already an admin") { }
}

public sealed class SuperAdminAlreadyExistsException : DomainException
{
    public SuperAdminAlreadyExistsException()
        : base("SuperAdmin already exists in the system") { }
}
```

---

## ✅ Testing Strategy

### Unit Tests
- AdminUser factory methods (CreateSuperAdmin, PromoteUserToAdmin)
- Permission validation logic
- Domain method tests (Suspend, Activate, DemoteToUser)
- AdminPermissions value object tests

### Integration Tests
- PromoteUserToAdmin command handler
- BlockUser/UnblockUser command handlers
- DismissTask/SuspendTask command handlers
- GetAllTasks query (admin version bypasses permissions)
- GetSystemStatistics query
- Admin action audit trail

### E2E Tests
- SuperAdmin initialization on system start
- Complete promote → demote flow
- Complete block → unblock flow
- Task dismissal flow with audit verification
- Admin action audit log verification

---

## 📝 Implementation Notes

### System Initialization
1. Create background service: `SuperAdminInitializationService`
2. On system start, check if SuperAdmin exists
3. If not, create SuperAdmin with secure generated password
4. Send credentials via email/SMS to system owner
5. Force password change on first login

### Permission Checks
1. Use `[Authorize(Roles = "SuperAdmin,Admin")]` on admin controllers
2. Within handlers, check specific permissions using `CanPerformAction()`
3. SuperAdmin always bypasses permission checks
4. Record all permission check results in audit log

### Audit Trail
1. Every admin action MUST be recorded
2. Include IP address and user agent
3. Store indefinitely (compliance requirement)
4. Provide audit report endpoints
5. Consider GDPR: Admin actions not subject to "right to be forgotten"

### Task Dismissal vs Deletion
- **Dismissal**: Soft delete, preserved for audit, admin can view
- **Deletion**: Hard delete, only via data retention policy
- Dismissed tasks appear in admin reports but not user views

### User Blocking vs Suspension
- **Blocking**: Complete access denial, login fails, used for policy violations
- **Suspension**: Temporary restriction, login succeeds with warning, limited access
- Blocked users must contact support
- Suspended users see suspension reason and expiry

---

## 🔗 Integration Points

### With Identity/AppUser
- AdminUser has FK to AppUser (authentication)
- Blocking updates AppUser.Status
- Admin role added to AppUser.Roles
- JWT includes "Admin" or "SuperAdmin" role claim

### With User Feature
- AdminUser optionally links to UserEntity (regular admins)
- SuperAdmin doesn't need UserEntity (system account)
- Promotion requires UserEntity (completed profile)

### With Task Feature
- Admin actions recorded in TaskHistory
- Dismissed/Suspended status added to TaskStatus enum
- Admin bypass for task visibility (ignores Master-SubUser permissions)

### Notifications
- Email notifications on block/suspend/dismiss
- In-app notifications for affected users
- Admin dashboard shows recent actions

---

## 🎉 Summary

The AdminUser feature provides:

✅ **Hierarchical Admin System**: SuperAdmin > Admin > User
✅ **User Management**: Block, unblock, suspend, activate users
✅ **Task Oversight**: View all tasks, dismiss/suspend tasks
✅ **Admin Management**: Promote, demote, suspend admins (SuperAdmin only)
✅ **Complete Audit Trail**: All actions tracked with IP and timestamp
✅ **Permission System**: Granular permissions with SuperAdmin override
✅ **System Initialization**: Auto-create SuperAdmin on first run
✅ **Security**: Protected SuperAdmin, permission validation
✅ **Compliance**: Audit log retention, GDPR considerations

This completes the comprehensive AdminUser feature specification! 🚀
