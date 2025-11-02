using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;
using TaskFlow.BuildingBlocks.Messaging.Adapters.MassTransit;
using TaskFlow.BuildingBlocks.Messaging.Configuration;

namespace TaskFlow.BuildingBlocks.Messaging.Extensions;

/// <summary>
/// Framework-agnostic extension methods for registering messaging services
/// Supports: MassTransit, Native RabbitMQ, Azure Service Bus, AWS SQS, etc.
/// </summary>
public static class MessagingExtensions
{
    // ============================================================================
    // GENERIC REGISTRATION (Framework-Agnostic)
    // ============================================================================

    /// <summary>
    /// Registers messaging with a custom IMessagePublisher implementation
    /// Use this for complete framework flexibility
    /// </summary>
    /// <typeparam name="TMessagePublisher">The message publisher implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMessaging<TMessagePublisher>(this IServiceCollection services)
        where TMessagePublisher : class, IMessagePublisher
    {
        services.AddScoped<IMessagePublisher, TMessagePublisher>();
        return services;
    }

    // ============================================================================
    // MASSTRANSIT (Most Common - Uses MassTransit Library)
    // ============================================================================

    /// <summary>
    /// Registers MassTransit-based messaging from configuration
    /// Reads from "Messaging" section in appsettings.json
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ?? new MessagingOptions();

        return services.AddMassTransitMessaging(options);
    }

    /// <summary>
    /// Registers MassTransit-based messaging with custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The messaging options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        MessagingOptions options)
    {
        // Register framework-agnostic IMessagePublisher
        services.AddScoped<IMessagePublisher, MassTransitMessagePublisher>();

        // Register MassTransit using the existing extension
        // (Keep backward compatibility with existing MassTransitExtensions)
        services.AddMassTransit(x =>
        {
            switch (options.Provider)
            {
                case MessagingProvider.RabbitMQ:
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(options.Host, options.Port, options.VirtualHost, h =>
                        {
                            h.Username(options.Username);
                            h.Password(options.Password);
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                    break;

                case MessagingProvider.AzureServiceBus:
                    x.UsingAzureServiceBus((context, cfg) =>
                    {
                        cfg.Host(options.ConnectionString);
                        cfg.ConfigureEndpoints(context);
                    });
                    break;

                case MessagingProvider.AmazonSQS:
                    x.UsingAmazonSqs((context, cfg) =>
                    {
                        cfg.Host(options.AwsRegion, h =>
                        {
                            if (!string.IsNullOrEmpty(options.AwsAccessKey))
                            {
                                h.AccessKey(options.AwsAccessKey);
                                h.SecretKey(options.AwsSecretKey);
                            }
                        });

                        cfg.ConfigureEndpoints(context);
                    });
                    break;

                case MessagingProvider.InMemory:
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                    break;
            }
        });

        return services;
    }

    // ============================================================================
    // NATIVE RABBITMQ (No MassTransit - Direct RabbitMQ.Client)
    // ============================================================================

    /// <summary>
    /// Registers native RabbitMQ messaging without MassTransit
    /// Uses RabbitMQ.Client library directly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">RabbitMQ connection string (e.g., "amqp://guest:guest@localhost:5672/")</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNativeRabbitMQMessaging(
        this IServiceCollection services,
        string connectionString)
    {
        // Register RabbitMQ connection
        services.AddSingleton<RabbitMQ.Client.IConnection>(sp =>
        {
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = new Uri(connectionString),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };
            return factory.CreateConnection();
        });

        // Register our framework-agnostic publisher
        services.AddSingleton<IMessagePublisher, Adapters.RabbitMQ.RabbitMQMessagePublisher>();

        return services;
    }

    // ============================================================================
    // AZURE SERVICE BUS (Native - No MassTransit)
    // ============================================================================

    /// <summary>
    /// Registers Azure Service Bus messaging without MassTransit
    /// Uses Azure.Messaging.ServiceBus SDK directly
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">Azure Service Bus connection string</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAzureServiceBusMessaging(
        this IServiceCollection services,
        string connectionString)
    {
        // Register Azure Service Bus client
        services.AddSingleton(sp =>
        {
            return new Azure.Messaging.ServiceBus.ServiceBusClient(connectionString);
        });

        // Register our framework-agnostic publisher
        services.AddSingleton<IMessagePublisher, Adapters.AzureServiceBus.AzureServiceBusMessagePublisher>();

        return services;
    }

    // ============================================================================
    // MESSAGE HANDLER REGISTRATION
    // ============================================================================

    /// <summary>
    /// Registers a framework-agnostic message handler
    /// Works with any messaging framework (MassTransit, native RabbitMQ, etc.)
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <typeparam name="THandler">The handler implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMessageHandler<TMessage, THandler>(this IServiceCollection services)
        where TMessage : class
        where THandler : class, IMessageHandler<TMessage>
    {
        services.AddScoped<IMessageHandler<TMessage>, THandler>();
        return services;
    }

    /// <summary>
    /// Registers a framework-agnostic message handler with MassTransit consumer adapter
    /// Bridges IMessageHandler to MassTransit's IConsumer
    /// </summary>
    /// <typeparam name="TMessage">The message type</typeparam>
    /// <typeparam name="THandler">The handler implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMassTransitMessageHandler<TMessage, THandler>(
        this IServiceCollection services)
        where TMessage : class
        where THandler : class, IMessageHandler<TMessage>
    {
        // Register framework-agnostic handler
        services.AddScoped<IMessageHandler<TMessage>, THandler>();

        // Register MassTransit consumer adapter
        services.AddScoped<MassTransitConsumerAdapter<TMessage>>();

        return services;
    }
}
