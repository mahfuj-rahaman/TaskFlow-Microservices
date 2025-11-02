using MassTransit;
using TaskFlow.BuildingBlocks.Messaging.Abstractions;

namespace TaskFlow.BuildingBlocks.Messaging.Adapters.MassTransit;

/// <summary>
/// MassTransit-specific implementation of IMessageContext
/// Wraps MassTransit's ConsumeContext to provide framework-agnostic access
/// </summary>
public sealed class MassTransitMessageContext : IMessageContext
{
    private readonly ConsumeContext _context;

    public MassTransitMessageContext(ConsumeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public Guid MessageId => _context.MessageId ?? Guid.Empty;

    public Guid? CorrelationId => _context.CorrelationId;

    public Guid? ConversationId => _context.ConversationId;

    public DateTime? SentTime => _context.SentTime;

    public IReadOnlyDictionary<string, object> Headers => _context.Headers.GetAll()
        .ToDictionary(x => x.Key, x => x.Value);

    public Uri? SourceAddress => _context.SourceAddress;

    public Uri? DestinationAddress => _context.DestinationAddress;

    public async Task RespondAsync<TResponse>(TResponse response, CancellationToken cancellationToken = default)
        where TResponse : class
    {
        await _context.RespondAsync(response);
    }
}
