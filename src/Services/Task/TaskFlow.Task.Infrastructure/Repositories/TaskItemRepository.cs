using Microsoft.EntityFrameworkCore;
using TaskFlow.Task.Application.Interfaces;
using TaskFlow.Task.Domain.Entities;
using TaskFlow.Task.Infrastructure.Persistence;

namespace TaskFlow.Task.Infrastructure.Repositories;

public sealed class TaskItemRepository : ITaskItemRepository
{
    private readonly TaskDbContext _context;

    public TaskItemRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<TaskItemEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskItemEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<TaskItemEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.TaskItems
            .Where(x => x.CreatedByUserId == userId || x.AssignedToUserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task AddAsync(TaskItemEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.TaskItems.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task UpdateAsync(TaskItemEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TaskItems.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task DeleteAsync(TaskItemEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TaskItems.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
