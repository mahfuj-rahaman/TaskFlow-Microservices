using TaskFlow.User.Domain.Entities;

namespace TaskFlow.User.Application.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfileEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserProfileEntity?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserProfileEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UserProfileEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserProfileEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserProfileEntity entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default);
}
