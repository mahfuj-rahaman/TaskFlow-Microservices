namespace TaskFlow.Identity.Domain.Exceptions;

/// <summary>
/// Exception thrown when credentials are invalid
/// </summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid credentials provided")
    {
    }

    public InvalidCredentialsException(string message)
        : base(message)
    {
    }
}
