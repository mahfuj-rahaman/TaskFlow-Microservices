using MassTransit;
using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// MassTransit adapter implementation of IMessagePublisher
/// Wraps MassTransit's IPublishEndpoint and ISendEndpointProvider
/// </summary>
public sealed class MassTransitMessagePublisher : IMessagePublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ISendEndpointProvider _sendEndpointProvider;

    public MassTransitMessagePublisher(
        IPublishEndpoint publishEndpoint,
        ISendEndpointProvider sendEndpointProvider)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _sendEndpointProvider = sendEndpointProvider ?? throw new ArgumentNullException(nameof(sendEndpointProvider));
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        await _publishEndpoint.Publish(message, cancellationToken);
    }

    public async Task PublishAsync<TMessage>(TMessage message, string destination, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // MassTransit doesn't support publishing to specific topics directly
        // We publish with a custom header instead
        await _publishEndpoint.Publish(message, context =>
        {
            context.Headers.Set("Destination", destination);
        }, cancellationToken);
    }

    public async Task SendAsync<TCommand>(TCommand command, string endpoint, CancellationToken cancellationToken = default)
        where TCommand : class
    {
        var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri(endpoint));
        await sendEndpoint.Send(command, cancellationToken);
    }
}
