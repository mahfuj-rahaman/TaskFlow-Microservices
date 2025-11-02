
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;
using TaskFlow.BuildingBlocks.Messaging.Adapters;
using TaskFlow.BuildingBlocks.Messaging.Configuration;

namespace TaskFlow.BuildingBlocks.Messaging.Extensions;

public static class AwsSqsExtensions
{
    public static IServiceCollection AddAwsSqsMessageBus(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
    {
        services.Configure<AwsSqsOptions>(configuration.GetSection(AwsSqsOptions.SectionName));

        services.AddSingleton<IMessageBus, AwsSqsAdapter>();
        services.AddHostedService(sp => (AwsSqsAdapter)sp.GetRequiredService<IMessageBus>());

        if (assemblies.Length > 0)
        {
            services.Scan(s => s
                .FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IMessageHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        }

        return services;
    }
}
