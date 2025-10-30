using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Task.Application.Interfaces;
using TaskFlow.Task.Infrastructure.Persistence;
using TaskFlow.Task.Infrastructure.Repositories;

namespace TaskFlow.Task.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<TaskDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("TaskDb");

            // Support both PostgreSQL and SQL Server
            var databaseProvider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";

            if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString,
                    b => b.MigrationsAssembly(typeof(TaskDbContext).Assembly.FullName));
            }
            else
            {
                options.UseNpgsql(connectionString,
                    b => b.MigrationsAssembly(typeof(TaskDbContext).Assembly.FullName));
            }

            // Enable sensitive data logging in development
            if (configuration.GetValue<bool>("EnableSensitiveDataLogging"))
            {
                options.EnableSensitiveDataLogging();
            }
        });

        // Add repositories
        services.AddScoped<ITaskRepository, TaskRepository>();

        // Add Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
