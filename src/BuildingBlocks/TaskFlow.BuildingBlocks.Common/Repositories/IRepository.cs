using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.Common.Pagination;

namespace TaskFlow.BuildingBlocks.Common.Repositories;

/// <summary>
/// Base repository interface for aggregate roots
/// </summary>
/// <typeparam name="TEntity">The aggregate root type</typeparam>
/// <typeparam name="TId">The identifier type</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an entity by its identifier
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of entities
    /// </summary>
    Task<PagedList<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}
