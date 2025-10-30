using TaskFlow.Task.Domain.Entities;

namespace TaskFlow.Task.Application.Interfaces;

/// <summary>
/// Repository interface for Task aggregate
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Gets a task by ID
    /// </summary>
    System.Threading.Tasks.Task<TaskEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tasks for a user
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tasks
    /// </summary>
    System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new task
    /// </summary>
    System.Threading.Tasks.Task AddAsync(TaskEntity task, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    void Update(TaskEntity task);

    /// <summary>
    /// Deletes a task
    /// </summary>
    void Delete(TaskEntity task);
}
