using TaskFlow.Task.Domain.Entities;

namespace TaskFlow.Task.Application.Interfaces;

public interface ITaskItemRepository
{
    System.Threading.Tasks.Task<TaskItemEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskItemEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskItemEntity>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task AddAsync(TaskItemEntity entity, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task UpdateAsync(TaskItemEntity entity, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(TaskItemEntity entity, CancellationToken cancellationToken = default);
}
