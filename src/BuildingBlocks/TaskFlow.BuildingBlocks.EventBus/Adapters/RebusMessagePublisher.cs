using TaskFlow.BuildingBlocks.EventBus.Abstractions;

namespace TaskFlow.BuildingBlocks.EventBus.Adapters;

/// <summary>
/// Rebus adapter implementation of IMessagePublisher
/// NOTE: This is a template - requires Rebus NuGet package to use
/// </summary>
public sealed class RebusMessagePublisher : IMessagePublisher
{
    // Uncomment when Rebus package is added:
    // private readonly IBus _bus;

    // public RebusMessagePublisher(IBus bus)
    // {
    //     _bus = bus ?? throw new ArgumentNullException(nameof(bus));
    // }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // Uncomment when Rebus package is added:
        // return _bus.Publish(message);

        throw new NotImplementedException("Rebus package not installed. Add NuGet: Rebus");
    }

    public Task PublishAsync<TMessage>(TMessage message, string destination, CancellationToken cancellationToken = default)
        where TMessage : class
    {
        // Uncomment when Rebus package is added:
        // var headers = new Dictionary<string, string> { ["Destination"] = destination };
        // return _bus.Publish(message, headers);

        throw new NotImplementedException("Rebus package not installed. Add NuGet: Rebus");
    }

    public Task SendAsync<TCommand>(TCommand command, string endpoint, CancellationToken cancellationToken = default)
        where TCommand : class
    {
        // Uncomment when Rebus package is added:
        // return _bus.Send(command);

        throw new NotImplementedException("Rebus package not installed. Add NuGet: Rebus");
    }
}
