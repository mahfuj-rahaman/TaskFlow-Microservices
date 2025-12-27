using System.Security.Cryptography;
using TaskFlow.Identity.Application.Interfaces;

namespace TaskFlow.Identity.Infrastructure.Services;

/// <summary>
/// Implementation of password hashing using BCrypt
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool Verify(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
