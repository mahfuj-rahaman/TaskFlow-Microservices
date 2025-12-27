namespace TaskFlow.Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when an account is locked
/// </summary>
public sealed class AccountLockedException : Exception
{
    public AccountLockedException(DateTime lockoutEnd)
        : base($"Account is locked until {lockoutEnd:yyyy-MM-dd HH:mm:ss} UTC")
    {
    }

    public AccountLockedException(string message)
        : base(message)
    {
    }
}
