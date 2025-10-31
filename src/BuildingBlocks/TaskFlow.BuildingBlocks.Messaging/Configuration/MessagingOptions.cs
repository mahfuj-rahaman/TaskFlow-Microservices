namespace TaskFlow.BuildingBlocks.Messaging.Configuration;

/// <summary>
/// Configuration options for messaging via MassTransit
/// Supports multiple transport providers (RabbitMQ, AWS SQS, Azure Service Bus, In-Memory)
/// </summary>
public sealed class MessagingOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Messaging";

    /// <summary>
    /// Messaging provider type (RabbitMQ, AmazonSQS, AzureServiceBus, InMemory)
    /// </summary>
    public MessagingProvider Provider { get; set; } = MessagingProvider.RabbitMQ;

    /// <summary>
    /// RabbitMQ host address
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ port
    /// </summary>
    public ushort Port { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ virtual host
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Username (RabbitMQ, Azure Service Bus)
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// Password (RabbitMQ, Azure Service Bus)
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Connection string (Azure Service Bus, AWS SQS)
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// AWS Region (for SQS)
    /// </summary>
    public string? AwsRegion { get; set; }

    /// <summary>
    /// AWS Access Key (for SQS)
    /// </summary>
    public string? AwsAccessKey { get; set; }

    /// <summary>
    /// AWS Secret Key (for SQS)
    /// </summary>
    public string? AwsSecretKey { get; set; }

    /// <summary>
    /// Retry policy configuration
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Outbox pattern configuration
    /// </summary>
    public OutboxOptions Outbox { get; set; } = new();

    /// <summary>
    /// Enable message scheduler for delayed/scheduled messages
    /// </summary>
    public bool UseMessageScheduler { get; set; } = true;
}

/// <summary>
/// Messaging provider types (avoids vendor lock-in)
/// </summary>
public enum MessagingProvider
{
    /// <summary>
    /// RabbitMQ message broker
    /// </summary>
    RabbitMQ,

    /// <summary>
    /// Amazon SQS (Simple Queue Service)
    /// </summary>
    AmazonSQS,

    /// <summary>
    /// Azure Service Bus
    /// </summary>
    AzureServiceBus,

    /// <summary>
    /// In-Memory (for testing)
    /// </summary>
    InMemory
}

/// <summary>
/// Retry policy configuration
/// </summary>
public sealed class RetryOptions
{
    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Initial retry interval in seconds
    /// </summary>
    public int InitialIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Increment for retry interval in seconds (for exponential backoff)
    /// </summary>
    public int IntervalIncrementSeconds { get; set; } = 5;
}

/// <summary>
/// Outbox pattern configuration for reliable message publishing
/// </summary>
public sealed class OutboxOptions
{
    /// <summary>
    /// Enable outbox pattern
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Delay between outbox queries in milliseconds
    /// </summary>
    public int QueryDelay { get; set; } = 100;

    /// <summary>
    /// Maximum number of messages to query from outbox
    /// </summary>
    public int QueryLimit { get; set; } = 100;
}
