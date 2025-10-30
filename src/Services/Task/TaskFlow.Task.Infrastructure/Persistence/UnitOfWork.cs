using TaskFlow.Task.Application.Interfaces;

namespace TaskFlow.Task.Infrastructure.Persistence;

/// <summary>
/// Unit of Work implementation using EF Core
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TaskDbContext _context;

    public UnitOfWork(TaskDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
