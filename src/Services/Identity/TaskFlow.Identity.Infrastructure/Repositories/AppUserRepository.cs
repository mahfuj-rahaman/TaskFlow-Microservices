using Microsoft.EntityFrameworkCore;
using TaskFlow.Identity.Application.Interfaces;
using TaskFlow.Identity.Domain.Entities;
using TaskFlow.Identity.Infrastructure.Persistence;

namespace TaskFlow.Identity.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AppUser
/// </summary>
public sealed class AppUserRepository : IAppUserRepository
{
    private readonly IdentityDbContext _context;

    public AppUserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<AppUserEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AppUserEntity>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AppUserEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AppUserEntity>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        AppUserEntity entity,
        CancellationToken cancellationToken = default)
    {
        await _context.Set<AppUserEntity>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        AppUserEntity entity,
        CancellationToken cancellationToken = default)
    {
        _context.Set<AppUserEntity>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        AppUserEntity entity,
        CancellationToken cancellationToken = default)
    {
        _context.Set<AppUserEntity>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<AppUserEntity>()
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
}
