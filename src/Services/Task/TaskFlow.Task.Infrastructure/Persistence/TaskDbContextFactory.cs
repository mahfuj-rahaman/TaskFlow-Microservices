using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskFlow.Task.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core migrations
/// </summary>
public class TaskDbContextFactory : IDesignTimeDbContextFactory<TaskDbContext>
{
    public TaskDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TaskDbContext>();

        // Use PostgreSQL by default for migrations
        // Connection string is just for migrations, actual connection comes from configuration
        optionsBuilder.UseNpgsql("Host=localhost;Database=taskflow_task;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly(typeof(TaskDbContext).Assembly.FullName));

        return new TaskDbContext(optionsBuilder.Options);
    }
}
