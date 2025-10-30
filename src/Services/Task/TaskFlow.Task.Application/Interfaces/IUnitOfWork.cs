namespace TaskFlow.Task.Application.Interfaces;

/// <summary>
/// Unit of Work pattern interface
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
