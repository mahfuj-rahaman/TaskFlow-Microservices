using Microsoft.EntityFrameworkCore;
using TaskFlow.User.Application.Interfaces;
using TaskFlow.User.Domain.Entities;
using TaskFlow.User.Infrastructure.Persistence;

namespace TaskFlow.User.Infrastructure.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly UserDbContext _context;

    public UserProfileRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<UserProfileEntity?> GetByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles.FirstOrDefaultAsync(x => x.AppUserId == appUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<UserProfileEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserProfileEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.UserProfiles.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserProfileEntity entity, CancellationToken cancellationToken = default)
    {
        _context.UserProfiles.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(UserProfileEntity entity, CancellationToken cancellationToken = default)
    {
        _context.UserProfiles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByAppUserIdAsync(Guid appUserId, CancellationToken cancellationToken = default)
    {
        return await _context.UserProfiles.AnyAsync(x => x.AppUserId == appUserId, cancellationToken);
    }
}
