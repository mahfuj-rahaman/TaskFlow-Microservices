namespace TaskFlow.User.Domain.Enums;

/// <summary>
/// Predefined permissions for fine-grained authorization
/// </summary>
public static class Permission
{
    // User Management
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersDelete = "users:delete";
    public const string UsersManageRoles = "users:manage-roles";

    // Profile Management
    public const string ProfileRead = "profile:read";
    public const string ProfileWrite = "profile:write";

    // System Administration
    public const string SystemSettings = "system:settings";
    public const string SystemLogs = "system:logs";
    public const string SystemBackup = "system:backup";

    /// <summary>
    /// All available permissions
    /// </summary>
    public static readonly string[] AllPermissions = new[]
    {
        UsersRead,
        UsersWrite,
        UsersDelete,
        UsersManageRoles,
        ProfileRead,
        ProfileWrite,
        SystemSettings,
        SystemLogs,
        SystemBackup
    };

    /// <summary>
    /// Checks if permission is valid
    /// </summary>
    public static bool IsValid(string permission)
    {
        return AllPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets permissions for a role
    /// </summary>
    public static string[] GetPermissionsForRole(string role)
    {
        return role.ToUpperInvariant() switch
        {
            "SUPERADMIN" => AllPermissions,
            "ADMIN" => new[]
            {
                UsersRead,
                UsersWrite,
                UsersDelete,
                UsersManageRoles,
                ProfileRead,
                ProfileWrite,
                SystemLogs
            },
            "MANAGER" => new[]
            {
                UsersRead,
                UsersWrite,
                ProfileRead,
                ProfileWrite
            },
            "USER" => new[]
            {
                ProfileRead,
                ProfileWrite
            },
            _ => Array.Empty<string>()
        };
    }
}
