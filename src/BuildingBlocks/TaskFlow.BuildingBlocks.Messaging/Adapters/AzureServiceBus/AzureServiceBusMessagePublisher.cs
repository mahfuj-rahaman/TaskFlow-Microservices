using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;

namespace TaskFlow.BuildingBlocks.Messaging.Adapters.AzureServiceBus;

/// <summary>
/// Azure Service Bus implementation of IMessagePublisher
/// Uses Azure.Messaging.ServiceBus SDK
///
/// SETUP INSTRUCTIONS:
/// 1. Install: dotnet add package Azure.Messaging.ServiceBus --version 7.18.1
/// 2. Register client:
///    builder.Services.AddSingleton(sp => {
///        return new ServiceBusClient(connectionString);
///    });
/// 3. Register publisher:
///    builder.Services.AddSingleton&lt;IMessagePublisher, AzureServiceBusMessagePublisher&gt;();
/// </summary>
public sealed class AzureServiceBusMessagePublisher : IMessagePublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ILogger<AzureServiceBusMessagePublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private const string TopicName = "taskflow-events";

    public AzureServiceBusMessagePublisher(
        ServiceBusClient client,
        ILogger<AzureServiceBusMessagePublisher> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var sender = _client.CreateSender(TopicName);

        try
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = new BinaryData(Encoding.UTF8.GetBytes(json));

            var serviceBusMessage = new ServiceBusMessage(body)
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/json",
                Subject = typeof(TMessage).Name,
                ApplicationProperties =
                {
                    ["MessageType"] = typeof(TMessage).FullName,
                    ["Timestamp"] = DateTime.UtcNow
                }
            };

            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "Published message {MessageType} to Azure Service Bus topic {Topic}",
                typeof(TMessage).Name,
                TopicName);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async Task SendAsync<TMessage>(Uri destinationAddress, TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var queueName = destinationAddress.AbsolutePath.TrimStart('/');
        var sender = _client.CreateSender(queueName);

        try
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = new BinaryData(Encoding.UTF8.GetBytes(json));

            var serviceBusMessage = new ServiceBusMessage(body)
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/json",
                Subject = typeof(TMessage).Name,
                ApplicationProperties =
                {
                    ["MessageType"] = typeof(TMessage).FullName,
                    ["Timestamp"] = DateTime.UtcNow
                }
            };

            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            _logger.LogInformation(
                "Sent message {MessageType} to Azure Service Bus queue {Queue}",
                typeof(TMessage).Name,
                queueName);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async Task SchedulePublishAsync<TMessage>(TMessage message, DateTime scheduledTime, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var sender = _client.CreateSender(TopicName);

        try
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = new BinaryData(Encoding.UTF8.GetBytes(json));

            var serviceBusMessage = new ServiceBusMessage(body)
            {
                MessageId = Guid.NewGuid().ToString(),
                ContentType = "application/json",
                Subject = typeof(TMessage).Name,
                ApplicationProperties =
                {
                    ["MessageType"] = typeof(TMessage).FullName,
                    ["Timestamp"] = DateTime.UtcNow
                }
            };

            // Azure Service Bus supports native message scheduling
            var sequenceNumber = await sender.ScheduleMessageAsync(
                serviceBusMessage,
                scheduledTime,
                cancellationToken);

            _logger.LogInformation(
                "Scheduled message {MessageType} to be published at {ScheduledTime} (sequence: {SequenceNumber})",
                typeof(TMessage).Name,
                scheduledTime,
                sequenceNumber);
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}
