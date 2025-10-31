using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;
using TaskFlow.BuildingBlocks.EventBus.Adapters;

namespace TaskFlow.BuildingBlocks.EventBus.Extensions;

/// <summary>
/// Extension methods for registering EventBus services
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Registers the EventBus with the service collection (without message publisher)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBus(this IServiceCollection services)
    {
        // Register EventBus as scoped (per request in web apps)
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }

    /// <summary>
    /// Registers the EventBus with MassTransit message publisher
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithMassTransit(this IServiceCollection services)
    {
        // Register MassTransit adapter
        services.AddScoped<IMessagePublisher, MassTransitMessagePublisher>();

        // Register EventBus
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }

    /// <summary>
    /// Registers the EventBus with a custom integration event mapper
    /// </summary>
    /// <typeparam name="TMapper">The integration event mapper implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBus<TMapper>(this IServiceCollection services)
        where TMapper : class, IIntegrationEventMapper
    {
        // Register the custom mapper
        services.AddScoped<IIntegrationEventMapper, TMapper>();

        // Register EventBus
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }

    /// <summary>
    /// Registers the EventBus with MassTransit and a custom integration event mapper
    /// </summary>
    /// <typeparam name="TMapper">The integration event mapper implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithMassTransit<TMapper>(this IServiceCollection services)
        where TMapper : class, IIntegrationEventMapper
    {
        // Register MassTransit adapter
        services.AddScoped<IMessagePublisher, MassTransitMessagePublisher>();

        // Register the custom mapper
        services.AddScoped<IIntegrationEventMapper, TMapper>();

        // Register EventBus
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }

    /// <summary>
    /// Registers the EventBus with a custom message publisher implementation
    /// </summary>
    /// <typeparam name="TPublisher">The message publisher implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithPublisher<TPublisher>(this IServiceCollection services)
        where TPublisher : class, IMessagePublisher
    {
        // Register custom message publisher
        services.AddScoped<IMessagePublisher, TPublisher>();

        // Register EventBus
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }
}
