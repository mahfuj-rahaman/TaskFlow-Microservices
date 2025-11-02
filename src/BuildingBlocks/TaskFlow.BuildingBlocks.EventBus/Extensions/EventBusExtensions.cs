using Microsoft.Extensions.DependencyInjection;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;
using TaskFlow.BuildingBlocks.EventBus.Adapters;

namespace TaskFlow.BuildingBlocks.EventBus.Extensions;

/// <summary>
/// Extension methods for registering EventBus services
/// Framework-agnostic - supports any event publisher and message publisher
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Registers the EventBus with a custom event publisher implementation
    /// Use this for framework-agnostic configuration
    /// </summary>
    /// <typeparam name="TEventPublisher">The in-process event publisher implementation (e.g., MediatREventPublisher)</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBus<TEventPublisher>(this IServiceCollection services)
        where TEventPublisher : class, IEventPublisher
    {
        // Register the event publisher (MediatR, Wolverine, custom, etc.)
        services.AddScoped<IEventPublisher, TEventPublisher>();

        // Register EventBus as scoped (per request in web apps)
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }

    /// <summary>
    /// Registers the EventBus with custom event publisher and message publisher implementations
    /// Use this for fully distributed event-driven architecture
    /// </summary>
    /// <typeparam name="TEventPublisher">The in-process event publisher implementation</typeparam>
    /// <typeparam name="TMessagePublisher">The distributed message publisher implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBus<TEventPublisher, TMessagePublisher>(this IServiceCollection services)
        where TEventPublisher : class, IEventPublisher
        where TMessagePublisher : class, IMessagePublisher
    {
        // Register the event publisher (in-process)
        services.AddScoped<IEventPublisher, TEventPublisher>();

        // Register the message publisher (distributed)
        services.AddScoped<IMessagePublisher, TMessagePublisher>();

        // Register EventBus
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }

    /// <summary>
    /// Registers the EventBus with custom event publisher, message publisher, and integration event mapper
    /// Use this for complete event-driven architecture with domain-to-integration event mapping
    /// </summary>
    /// <typeparam name="TEventPublisher">The in-process event publisher implementation</typeparam>
    /// <typeparam name="TMessagePublisher">The distributed message publisher implementation</typeparam>
    /// <typeparam name="TEventMapper">The integration event mapper implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBus<TEventPublisher, TMessagePublisher, TEventMapper>(
        this IServiceCollection services)
        where TEventPublisher : class, IEventPublisher
        where TMessagePublisher : class, IMessagePublisher
        where TEventMapper : class, IIntegrationEventMapper
    {
        // Register the event publisher (in-process)
        services.AddScoped<IEventPublisher, TEventPublisher>();

        // Register the message publisher (distributed)
        services.AddScoped<IMessagePublisher, TMessagePublisher>();

        // Register the event mapper (domain → integration)
        services.AddScoped<IIntegrationEventMapper, TEventMapper>();

        // Register EventBus
        services.AddScoped<IEventBus, EventBus>();

        return services;
    }

    // ============================================================================
    // Convenience methods for common scenarios
    // ============================================================================

    #region In-Memory Event Publisher (Testing & Simple Apps)

    /// <summary>
    /// Registers the EventBus with in-memory event publisher (no framework dependencies)
    /// Perfect for testing, simple applications, or learning
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithInMemory(this IServiceCollection services)
    {
        return services.AddEventBus<InMemoryEventPublisher>();
    }

    #endregion

    #region MediatR Event Publisher

    /// <summary>
    /// Registers the EventBus with MediatR for in-process events (no distributed events)
    /// Convenience method for MediatR users
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithMediatR(this IServiceCollection services)
    {
        return services.AddEventBus<MediatREventPublisher>();
    }

    /// <summary>
    /// Registers the EventBus with MediatR and MassTransit
    /// Convenience method for MediatR + MassTransit users
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithMediatRAndMassTransit(this IServiceCollection services)
    {
        return services.AddEventBus<MediatREventPublisher, MassTransitMessagePublisher>();
    }

    /// <summary>
    /// Registers the EventBus with MediatR, MassTransit, and custom integration event mapper
    /// Convenience method for MediatR + MassTransit users with custom mapping
    /// </summary>
    /// <typeparam name="TEventMapper">The integration event mapper implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithMediatRAndMassTransit<TEventMapper>(
        this IServiceCollection services)
        where TEventMapper : class, IIntegrationEventMapper
    {
        return services.AddEventBus<MediatREventPublisher, MassTransitMessagePublisher, TEventMapper>();
    }

    #endregion

    #region Wolverine Event Publisher

    /// <summary>
    /// Registers the EventBus with Wolverine for in-process events
    /// Wolverine is a high-performance alternative to MediatR
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithWolverine(this IServiceCollection services)
    {
        return services.AddEventBus<WolverineEventPublisher>();
    }

    /// <summary>
    /// Registers the EventBus with Wolverine and MassTransit
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithWolverineAndMassTransit(this IServiceCollection services)
    {
        return services.AddEventBus<WolverineEventPublisher, MassTransitMessagePublisher>();
    }

    #endregion

    #region Brighter Event Publisher

    /// <summary>
    /// Registers the EventBus with Brighter (Paramore.Brighter) for in-process events
    /// Brighter implements the Command Processor and Service Activator patterns
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithBrighter(this IServiceCollection services)
    {
        return services.AddEventBus<BrighterEventPublisher>();
    }

    /// <summary>
    /// Registers the EventBus with Brighter and MassTransit
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithBrighterAndMassTransit(this IServiceCollection services)
    {
        return services.AddEventBus<BrighterEventPublisher, MassTransitMessagePublisher>();
    }

    #endregion

    #region NServiceBus Event Publisher

    /// <summary>
    /// Registers the EventBus with NServiceBus for in-process and distributed events
    /// NServiceBus is an enterprise-grade service bus
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddEventBusWithNServiceBus(this IServiceCollection services)
    {
        return services.AddEventBus<NServiceBusEventPublisher, NServiceBusMessagePublisher>();
    }

    #endregion
}
