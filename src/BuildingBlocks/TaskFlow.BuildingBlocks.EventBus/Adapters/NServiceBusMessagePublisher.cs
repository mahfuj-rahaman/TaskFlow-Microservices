using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// NServiceBus adapter implementation of IMessagePublisher
/// NOTE: This is a template - requires NServiceBus NuGet package to use
/// </summary>
public sealed class NServiceBusMessagePublisher : IMessagePublisher
{
    // Uncomment when NServiceBus package is added:
    // private readonly IMessageSession _messageSession;

    // public NServiceBusMessagePublisher(IMessageSession messageSession)
    // {
    //     _messageSession = messageSession ?? throw new ArgumentNullException(nameof(messageSession));
    // }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // Uncomment when NServiceBus package is added:
        // return _messageSession.Publish(message, cancellationToken);

        throw new NotImplementedException("NServiceBus package not installed. Add NuGet: NServiceBus");
    }

    public Task PublishAsync<TMessage>(TMessage message, string destination, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // Uncomment when NServiceBus package is added:
        // var options = new PublishOptions();
        // options.SetDestination(destination);
        // return _messageSession.Publish(message, options, cancellationToken);

        throw new NotImplementedException("NServiceBus package not installed. Add NuGet: NServiceBus");
    }

    public Task SendAsync<TCommand>(TCommand command, string endpoint, CancellationToken cancellationToken = default)
        where TCommand : class
    {
        // Uncomment when NServiceBus package is added:
        // var options = new SendOptions();
        // options.SetDestination(endpoint);
        // return _messageSession.Send(command, options, cancellationToken);

        throw new NotImplementedException("NServiceBus package not installed. Add NuGet: NServiceBus");
    }
}
