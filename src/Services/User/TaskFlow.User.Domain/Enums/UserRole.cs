namespace TaskFlow.User.Domain.Enums;

/// <summary>
/// Predefined user roles
/// </summary>
public static class UserRole
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Guest = "Guest";

    /// <summary>
    /// All available roles
    /// </summary>
    public static readonly string[] AllRoles = new[]
    {
        SuperAdmin,
        Admin,
        Manager,
        User,
        Guest
    };

    /// <summary>
    /// Checks if role is valid
    /// </summary>
    public static bool IsValid(string role)
    {
        return AllRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
