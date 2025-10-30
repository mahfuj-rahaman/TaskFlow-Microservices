using Microsoft.EntityFrameworkCore;
using TaskFlow.Task.Application.Interfaces;
using TaskFlow.Task.Domain.Entities;
using TaskFlow.Task.Infrastructure.Persistence;

namespace TaskFlow.Task.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ITaskRepository
/// </summary>
public sealed class TaskRepository : ITaskRepository
{
    private readonly TaskDbContext _context;

    public TaskRepository(TaskDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<TaskEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Tasks
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async System.Threading.Tasks.Task AddAsync(
        TaskEntity task,
        CancellationToken cancellationToken = default)
    {
        await _context.Tasks.AddAsync(task, cancellationToken);
    }

    public void Update(TaskEntity task)
    {
        _context.Tasks.Update(task);
    }

    public void Delete(TaskEntity task)
    {
        _context.Tasks.Remove(task);
    }
}
