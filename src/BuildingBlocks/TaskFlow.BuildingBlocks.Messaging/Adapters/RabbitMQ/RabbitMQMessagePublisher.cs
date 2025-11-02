using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;

namespace TaskFlow.BuildingBlocks.Messaging.Adapters.RabbitMQ;

/// <summary>
/// Native RabbitMQ implementation of IMessagePublisher
/// Uses RabbitMQ.Client directly without MassTransit
///
/// SETUP INSTRUCTIONS:
/// 1. Install: dotnet add package RabbitMQ.Client --version 6.8.1
/// 2. Register connection:
///    builder.Services.AddSingleton&lt;IConnection&gt;(sp => {
///        var factory = new ConnectionFactory { HostName = "localhost" };
///        return factory.CreateConnection();
///    });
/// 3. Register publisher:
///    builder.Services.AddSingleton&lt;IMessagePublisher, RabbitMQMessagePublisher&gt;();
/// </summary>
public sealed class RabbitMQMessagePublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQMessagePublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string ExchangeName = "taskflow.events";

    public RabbitMQMessagePublisher(
        IConnection connection,
        ILogger<RabbitMQMessagePublisher> logger)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _channel = _connection.CreateModel();

        // Declare topic exchange for pub/sub
        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var messageType = typeof(TMessage);
        var routingKey = messageType.Name;

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // Persistent
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = messageType.FullName;

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Published message {MessageType} to RabbitMQ exchange {Exchange} with routing key {RoutingKey}",
            messageType.Name,
            ExchangeName,
            routingKey);

        return Task.CompletedTask;
    }

    public Task SendAsync<TMessage>(Uri destinationAddress, TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var queueName = destinationAddress.AbsolutePath.TrimStart('/');

        // Declare queue if it doesn't exist
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // Persistent
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = typeof(TMessage).FullName;

        _channel.BasicPublish(
            exchange: string.Empty, // Direct to queue
            routingKey: queueName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Sent message {MessageType} to RabbitMQ queue {Queue}",
            typeof(TMessage).Name,
            queueName);

        return Task.CompletedTask;
    }

    public Task SchedulePublishAsync<TMessage>(TMessage message, DateTime scheduledTime, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // RabbitMQ doesn't have built-in scheduling
        // Requires rabbitmq-delayed-message-exchange plugin or custom implementation
        var delayMs = (scheduledTime - DateTime.UtcNow).TotalMilliseconds;

        if (delayMs <= 0)
        {
            return PublishAsync(message, cancellationToken);
        }

        var messageType = typeof(TMessage);
        var routingKey = messageType.Name;

        var json = JsonSerializer.Serialize(message, _jsonOptions);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // Persistent
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = messageType.FullName;

        // Use x-delay header (requires delayed message exchange plugin)
        properties.Headers = new Dictionary<string, object>
        {
            { "x-delay", (int)delayMs }
        };

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        _logger.LogInformation(
            "Scheduled message {MessageType} to be published at {ScheduledTime} (delay: {DelayMs}ms)",
            messageType.Name,
            scheduledTime,
            delayMs);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
    }
}
