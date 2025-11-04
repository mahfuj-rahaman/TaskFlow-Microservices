
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
        var section = configuration.GetSection(AwsSqsOptions.SectionName);
        services.Configure<AwsSqsOptions>(options => section.Bind(options));

        services.AddSingleton<IMessageBus, AwsSqsAdapter>();
        services.AddHostedService(sp => (AwsSqsAdapter)sp.GetRequiredService<IMessageBus>());

        // Register message handlers from provided assemblies
        if (assemblies.Length > 0)
        {
            foreach (var assembly in assemblies)
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract &&
                           t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>)));

                foreach (var handlerType in handlerTypes)
                {
                    var interfaceTypes = handlerType.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMessageHandler<>));

                    foreach (var interfaceType in interfaceTypes)
                    {
                        services.AddScoped(interfaceType, handlerType);
                    }
                }
            }
        }

        return services;
    }
}
