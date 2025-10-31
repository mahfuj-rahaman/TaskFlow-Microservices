using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Messaging.Configuration;

namespace TaskFlow.BuildingBlocks.Messaging.Extensions;

/// <summary>
/// Extension methods for configuring MassTransit with multiple transport providers
/// Supports: RabbitMQ, AWS SQS, Azure Service Bus, In-Memory
/// </summary>
public static class MassTransitExtensions
{
    /// <summary>
    /// Adds MassTransit with transport provider based on configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="configureConsumers">Optional action to configure consumers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        // Bind messaging options from configuration
        var options = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ?? new MessagingOptions();

        return services.AddMassTransitMessaging(options, configureConsumers);
    }

    /// <summary>
    /// Adds MassTransit with transport provider based on custom options
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="options">The messaging options</param>
    /// <param name="configureConsumers">Optional action to configure consumers</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        MessagingOptions options,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        // Configure MassTransit based on provider
        services.AddMassTransit(x =>
        {
            // Register consumers if provided
            configureConsumers?.Invoke(x);

            // Configure transport based on provider type
            switch (options.Provider)
            {
                case MessagingProvider.RabbitMQ:
                    ConfigureRabbitMq(x, options);
                    break;

                case MessagingProvider.AmazonSQS:
                    ConfigureAmazonSqs(x, options);
                    break;

                case MessagingProvider.AzureServiceBus:
                    ConfigureAzureServiceBus(x, options);
                    break;

                case MessagingProvider.InMemory:
                    ConfigureInMemory(x, options);
                    break;

                default:
                    throw new ArgumentException($"Unsupported messaging provider: {options.Provider}");
            }

            // Add message scheduler for delayed/scheduled messages
            if (options.UseMessageScheduler)
            {
                x.AddDelayedMessageScheduler();
            }
        });

        return services;
    }

    /// <summary>
    /// Configures RabbitMQ transport
    /// </summary>
    private static void ConfigureRabbitMq(IBusRegistrationConfigurator configurator, MessagingOptions options)
    {
        configurator.UsingRabbitMq((context, cfg) =>
        {
            // Configure RabbitMQ host
            cfg.Host(options.Host, options.Port, options.VirtualHost, h =>
            {
                h.Username(options.Username);
                h.Password(options.Password);
            });

            // Configure retry policy
            cfg.UseMessageRetry(r => r.Incremental(
                options.Retry.RetryCount,
                TimeSpan.FromSeconds(options.Retry.InitialIntervalSeconds),
                TimeSpan.FromSeconds(options.Retry.IntervalIncrementSeconds)));

            // Configure endpoints (auto-discovery based on consumers)
            cfg.ConfigureEndpoints(context);
        });
    }

    /// <summary>
    /// Configures AWS SQS transport
    /// </summary>
    private static void ConfigureAmazonSqs(IBusRegistrationConfigurator configurator, MessagingOptions options)
    {
        configurator.UsingAmazonSqs((context, cfg) =>
        {
            // Configure AWS SQS
            cfg.Host(options.AwsRegion ?? "us-east-1", h =>
            {
                if (!string.IsNullOrEmpty(options.AwsAccessKey) && !string.IsNullOrEmpty(options.AwsSecretKey))
                {
                    h.AccessKey(options.AwsAccessKey);
                    h.SecretKey(options.AwsSecretKey);
                }
                // Otherwise uses IAM role or environment variables
            });

            // Configure retry policy
            cfg.UseMessageRetry(r => r.Incremental(
                options.Retry.RetryCount,
                TimeSpan.FromSeconds(options.Retry.InitialIntervalSeconds),
                TimeSpan.FromSeconds(options.Retry.IntervalIncrementSeconds)));

            // Configure endpoints (auto-discovery based on consumers)
            cfg.ConfigureEndpoints(context);
        });
    }

    /// <summary>
    /// Configures Azure Service Bus transport
    /// </summary>
    private static void ConfigureAzureServiceBus(IBusRegistrationConfigurator configurator, MessagingOptions options)
    {
        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            throw new ArgumentException("ConnectionString is required for Azure Service Bus");
        }

        configurator.UsingAzureServiceBus((context, cfg) =>
        {
            // Configure Azure Service Bus
            cfg.Host(options.ConnectionString);

            // Configure retry policy
            cfg.UseMessageRetry(r => r.Incremental(
                options.Retry.RetryCount,
                TimeSpan.FromSeconds(options.Retry.InitialIntervalSeconds),
                TimeSpan.FromSeconds(options.Retry.IntervalIncrementSeconds)));

            // Configure endpoints (auto-discovery based on consumers)
            cfg.ConfigureEndpoints(context);
        });
    }

    /// <summary>
    /// Configures In-Memory transport (for testing)
    /// </summary>
    private static void ConfigureInMemory(IBusRegistrationConfigurator configurator, MessagingOptions options)
    {
        configurator.UsingInMemory((context, cfg) =>
        {
            // Configure retry policy
            cfg.UseMessageRetry(r => r.Incremental(
                options.Retry.RetryCount,
                TimeSpan.FromSeconds(options.Retry.InitialIntervalSeconds),
                TimeSpan.FromSeconds(options.Retry.IntervalIncrementSeconds)));

            // Configure endpoints (auto-discovery based on consumers)
            cfg.ConfigureEndpoints(context);
        });
    }
}
