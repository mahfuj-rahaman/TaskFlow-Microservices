using TaskFlow.Identity.Domain.Entities;

namespace TaskFlow.Identity.Application.Interfaces;

/// <summary>
/// Repository interface for AppUser aggregate
/// </summary>
public interface IAppUserRepository
{
    /// <summary>
    /// Gets a AppUser by ID
    /// </summary>
    Task<AppUserEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all AppUsers
    /// </summary>
    Task<IReadOnlyList<AppUserEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new AppUser
    /// </summary>
    Task AddAsync(AppUserEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing AppUser
    /// </summary>
    Task UpdateAsync(AppUserEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a AppUser
    /// </summary>
    Task DeleteAsync(AppUserEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a AppUser exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a AppUser by email
    /// </summary>
    Task<AppUserEntity?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a AppUser by username
    /// </summary>
    Task<AppUserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if email already exists
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if username already exists
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}
