
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskFlow.BuildingBlocks.Common.Domain;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;
using TaskFlow.BuildingBlocks.EventBus.Configuration;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

public class AwsSnsEventBusAdapter : IEventBus
{
    private readonly IEventPublisher _inProcessPublisher; // For in-process events
    private readonly AmazonSimpleNotificationServiceClient _snsClient;
    private readonly AwsSnsOptions _options;
    private readonly ILogger<AwsSnsEventBusAdapter> _logger;

    public AwsSnsEventBusAdapter(
        IEventPublisher inProcessPublisher,
        IOptions<AwsSnsOptions> options,
        ILogger<AwsSnsEventBusAdapter> logger)
    {
        _inProcessPublisher = inProcessPublisher;
        _options = options.Value;
        _logger = logger;
        _snsClient = new AmazonSimpleNotificationServiceClient(_options.AccessKey, _options.SecretKey, Amazon.RegionEndpoint.GetBySystemName(_options.Region));
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IDomainEvent
    {
        // 1. Publish in-process first
        await _inProcessPublisher.PublishAsync(@event, cancellationToken);

        // 2. Publish to AWS SNS for distributed listeners
        await PublishToSnsAsync(@event, cancellationToken);
    }

    public async Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default)
    {
        // 1. Publish in-process first
        await _inProcessPublisher.PublishAsync(events, cancellationToken);

        // 2. Publish to AWS SNS
        foreach (var @event in events)
        {
            await PublishToSnsAsync(@event, cancellationToken);
        }
    }

    private async Task PublishToSnsAsync(IDomainEvent @event, CancellationToken cancellationToken)
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(@event, @event.GetType());
            var eventType = @event.GetType().Name;

            var request = new PublishRequest
            {
                TopicArn = _options.TopicArn,
                Message = messageBody,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "EventType", new MessageAttributeValue { DataType = "String", StringValue = eventType } }
                }
            };

            _logger.LogInformation("Publishing event {EventType} to SNS topic {TopicArn}", eventType, _options.TopicArn);
            await _snsClient.PublishAsync(request, cancellationToken);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType} to SNS", @event.GetType().Name);
            // Depending on requirements, might need retry logic or dead-letter queue
            throw;
        }
    }
}
