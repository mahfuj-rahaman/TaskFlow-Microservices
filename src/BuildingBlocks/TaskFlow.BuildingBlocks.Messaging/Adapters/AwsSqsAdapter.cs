
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;
using TaskFlow.BuildingBlocks.Messaging.Configuration;

namespace TaskFlow.BuildingBlocks.Messaging.Adapters;

public class AwsSqsAdapter : IMessageBus, IHostedService
{
    private readonly ILogger<AwsSqsAdapter> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AmazonSQSClient _sqsClient;
    private readonly AwsSqsOptions _options;
    private CancellationTokenSource _cancellationTokenSource = new();
    private Task? _pollingTask;

    public bool IsRunning { get; private set; }

    public AwsSqsAdapter(
        IOptions<AwsSqsOptions> options,
        ILogger<AwsSqsAdapter> logger,
        IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _sqsClient = new AmazonSQSClient(_options.AccessKey, _options.SecretKey, Amazon.RegionEndpoint.GetBySystemName(_options.Region));
    }

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning) return Task.CompletedTask;

        _logger.LogInformation("Starting AWS SQS message bus...");
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _pollingTask = Task.Run(() => PollForMessagesAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        IsRunning = true;
        _logger.LogInformation("AWS SQS message bus started.");
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRunning || _pollingTask == null) return;

        _logger.LogInformation("Stopping AWS SQS message bus...");
        _cancellationTokenSource.Cancel();
        await _pollingTask;
        IsRunning = false;
        _logger.LogInformation("AWS SQS message bus stopped.");
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        var queueUrl = _options.QueueUrl; // Assuming a default queue for publish
        await SendInternalAsync(queueUrl, message, cancellationToken);
    }

    public async Task SendAsync<TMessage>(Uri destinationAddress, TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        await SendInternalAsync(destinationAddress.ToString(), message, cancellationToken);
    }

    public async Task SchedulePublishAsync<TMessage>(TMessage message, DateTime scheduledTime, CancellationToken cancellationToken = default) where TMessage : class
    {
        var delay = scheduledTime - DateTime.UtcNow;
        if (delay.TotalSeconds <= 0) {
             await PublishAsync(message, cancellationToken);
             return;
        }

        // SQS max delay is 15 minutes
        var delaySeconds = (int)Math.Min(delay.TotalSeconds, 900);
        
        var queueUrl = _options.QueueUrl;
        await SendInternalAsync(queueUrl, message, cancellationToken, delaySeconds);
    }

    private async Task SendInternalAsync<TMessage>(string queueUrl, TMessage message, CancellationToken cancellationToken, int delaySeconds = 0) where TMessage : class
    {
        try
        {
            var messageBody = JsonSerializer.Serialize(message);
            var messageType = message.GetType().AssemblyQualifiedName;

            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                DelaySeconds = delaySeconds,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    { "MessageType", new MessageAttributeValue { DataType = "String", StringValue = messageType } }
                }
            };

            await _sqsClient.SendMessageAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to SQS queue {QueueUrl}", queueUrl);
            throw;
        }
    }

    private async Task PollForMessagesAsync(CancellationToken cancellationToken)
    {
        var receiveRequest = new ReceiveMessageRequest
        {
            QueueUrl = _options.QueueUrl,
            MaxNumberOfMessages = _options.MaxMessages,
            WaitTimeSeconds = _options.WaitTimeSeconds,
            MessageAttributeNames = new List<string> { "All" }
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var response = await _sqsClient.ReceiveMessageAsync(receiveRequest, cancellationToken);

                foreach (var message in response.Messages)
                {
                    await ProcessMessageAsync(message, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Message polling was cancelled.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error polling SQS queue {QueueUrl}", _options.QueueUrl);
                await Task.Delay(5000, cancellationToken); // Wait before retrying
            }
        }
    }

    private async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            if (!message.MessageAttributes.TryGetValue("MessageType", out var messageTypeAttribute))
            {
                _logger.LogWarning("Message {MessageId} does not have a MessageType attribute.", message.MessageId);
                return; // Or move to DLQ
            }

            var messageType = Type.GetType(messageTypeAttribute.StringValue);
            if (messageType == null)
            {
                _logger.LogError("Could not find type for message {MessageId}: {MessageType}", message.MessageId, messageTypeAttribute.StringValue);
                return; // Or move to DLQ
            }

            var messageBody = JsonSerializer.Deserialize(message.Body, messageType);
            if (messageBody == null)
            {
                _logger.LogError("Failed to deserialize message {MessageId}", message.MessageId);
                return; // Or move to DLQ
            }

            using var scope = _serviceProvider.CreateScope();
            var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);
            var handler = scope.ServiceProvider.GetService(handlerType);

            if (handler == null)
            {
                _logger.LogWarning("No handler found for message type {MessageType}", messageType.Name);
                return; // Or move to DLQ
            }

            var context = new MessageContext(); // Basic context, can be expanded

            await (Task)handler.GetType().GetMethod("HandleAsync").Invoke(handler, new[] { messageBody, context, cancellationToken });

            // If successful, delete the message
            await _sqsClient.DeleteMessageAsync(_options.QueueUrl, message.ReceiptHandle, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageId}", message.MessageId);
            // Consider moving to a DLQ instead of just logging
        }
    }
}

// Basic IMessageContext implementation
public class MessageContext : IMessageContext
{
    public Guid MessageId { get; } = Guid.NewGuid();
    public DateTime SentTime { get; } = DateTime.UtcNow;
    public Dictionary<string, object> Headers { get; } = new Dictionary<string, object>();
}

