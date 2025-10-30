namespace TaskFlow.User.Domain.Enums;

/// <summary>
/// User account status
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User account is active
    /// </summary>
    Active = 1,

    /// <summary>
    /// User account is inactive
    /// </summary>
    Inactive = 2,

    /// <summary>
    /// User account is suspended
    /// </summary>
    Suspended = 3
}