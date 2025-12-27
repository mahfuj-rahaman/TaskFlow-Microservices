namespace TaskFlow.Identity.Application.Interfaces;

/// <summary>
/// Interface for password hashing operations
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    bool Verify(string password, string passwordHash);
}
