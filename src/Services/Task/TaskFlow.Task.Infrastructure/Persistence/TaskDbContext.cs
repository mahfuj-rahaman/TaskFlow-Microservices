using Microsoft.EntityFrameworkCore;
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.Task.Domain.Entities;

namespace TaskFlow.Task.Infrastructure.Persistence;

/// <summary>
/// Database context for Task service
/// </summary>
public sealed class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Get all domain events before saving
        var domainEvents = ChangeTracker.Entries<Entity<Guid>>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Save changes
        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Publish domain events via MediatR or message bus
        // For now, we'll just clear them
        foreach (var entity in ChangeTracker.Entries<Entity<Guid>>().Select(e => e.Entity))
        {
            entity.ClearDomainEvents();
        }

        return result;
    }
}
