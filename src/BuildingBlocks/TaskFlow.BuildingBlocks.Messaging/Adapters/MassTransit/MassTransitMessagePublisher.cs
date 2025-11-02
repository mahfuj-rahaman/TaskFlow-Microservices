using MassTransit;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;

namespace TaskFlow.BuildingBlocks.Messaging.Adapters.MassTransit;

/// <summary>
/// MassTransit-specific implementation of IMessagePublisher
/// Adapts MassTransit's IBus and IPublishEndpoint to our framework-agnostic interface
/// </summary>
public sealed class MassTransitMessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;

    public MassTransitMessagePublisher(IBus bus)
    {
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        await _bus.Publish(message, cancellationToken);
    }

    public async Task SendAsync<TMessage>(Uri destinationAddress, TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        var endpoint = await _bus.GetSendEndpoint(destinationAddress);
        await endpoint.Send(message, cancellationToken);
    }

    public async Task SchedulePublishAsync<TMessage>(TMessage message, DateTime scheduledTime, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // Get message scheduler from bus
        var scheduler = _bus.CreateMessageScheduler();
        await scheduler.SchedulePublish(scheduledTime, message, cancellationToken);
    }
}
