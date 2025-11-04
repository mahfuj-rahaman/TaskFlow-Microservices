using Microsoft.EntityFrameworkCore;
using TaskFlow.Identity.Domain.Entities;
using System.Reflection;

namespace TaskFlow.Identity.Infrastructure.Persistence;

/// <summary>
/// DbContext for Identity microservice
/// </summary>
public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// AppUsers table
    /// </summary>
    public DbSet<AppUserEntity> AppUsers => Set<AppUserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
