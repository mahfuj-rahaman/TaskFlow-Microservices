# EventBus BuildingBlock Specification

**Project**: TaskFlow.BuildingBlocks.EventBus
**Version**: 1.0.0
**Last Updated**: 2025-11-01
**Status**: Specification

---

## 📋 Overview

The EventBus BuildingBlock provides a unified abstraction for publishing and subscribing to domain events, supporting both in-process (MediatR) and distributed (RabbitMQ/MassTransit) event handling.

## 🎯 Purpose

- Publish domain events from aggregates
- Handle events in-process for immediate consistency
- Publish events to message bus for eventual consistency
- Support both synchronous and asynchronous event handling
- Provide integration event transformation

## 🏗️ Architecture

```
Domain Event → EventBus → [In-Process Handler (MediatR)]
                       → [Integration Event → Message Bus (RabbitMQ)]
```

## 📦 Components

### 1. Core Interfaces

#### IEventBus
```csharp
public interface IEventBus
{
    /// <summary>
    /// Publishes an event (in-process and/or distributed)
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;

    /// <summary>
    /// Publishes multiple events
    /// </summary>
    Task PublishAsync(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
}
```

#### IIntegrationEventMapper
```csharp
public interface IIntegrationEventMapper
{
    /// <summary>
    /// Maps domain event to integration event (for distributed publishing)
    /// </summary>
    IIntegrationEvent? Map(IDomainEvent domainEvent);
}
```

#### IIntegrationEvent
```csharp
public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}
```

### 2. Implementation Classes

#### EventBus (Main Implementation)
```csharp
public sealed class EventBus : IEventBus
{
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint; // MassTransit
    private readonly IIntegrationEventMapper _eventMapper;
    private readonly ILogger<EventBus> _logger;

    // Implementation details
}
```

#### IntegrationEventMapper
```csharp
public sealed class IntegrationEventMapper : IIntegrationEventMapper
{
    // Maps domain events to integration events
    // Uses strategy pattern for different event types
}
```

### 3. Base Classes

#### IntegrationEventBase
```csharp
public abstract record IntegrationEventBase : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
```

## 📝 Usage Examples

### Publishing Domain Events from Aggregate

```csharp
public sealed class OrderEntity : AggregateRoot<Guid>
{
    public void Complete()
    {
        Status = OrderStatus.Completed;
        RaiseDomainEvent(new OrderCompletedDomainEvent(Id, CustomerId, TotalAmount));
    }
}

// In CommandHandler
public async Task<Result> Handle(CompleteOrderCommand request, CancellationToken ct)
{
    var order = await _repository.GetByIdAsync(request.OrderId, ct);
    order.Complete();

    await _repository.UpdateAsync(order, ct);

    // Publish all domain events
    await _eventBus.PublishAsync(order.DomainEvents, ct);
    order.ClearDomainEvents();

    return Result.Success();
}
```

### Creating Integration Events

```csharp
// Domain Event
public sealed record OrderCompletedDomainEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}

// Integration Event (for distributed systems)
public sealed record OrderCompletedIntegrationEvent : IntegrationEventBase
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public override string EventType => "order.completed";
}

// Mapper
public class OrderEventMapper : IIntegrationEventMapper
{
    public IIntegrationEvent? Map(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            OrderCompletedDomainEvent e => new OrderCompletedIntegrationEvent
            {
                OrderId = e.OrderId,
                CustomerId = e.CustomerId,
                TotalAmount = e.TotalAmount
            },
            _ => null
        };
    }
}
```

### Handling Events

```csharp
// In-Process Handler (same service)
public sealed class OrderCompletedDomainEventHandler
    : INotificationHandler<OrderCompletedDomainEvent>
{
    public async Task Handle(OrderCompletedDomainEvent notification, CancellationToken ct)
    {
        // Update read models, send notifications, etc.
    }
}

// Distributed Handler (different service - User Service)
public sealed class OrderCompletedIntegrationEventConsumer
    : IConsumer<OrderCompletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedIntegrationEvent> context)
    {
        // Update user statistics, loyalty points, etc.
    }
}
```

## ⚙️ Configuration

### DependencyInjection Registration

```csharp
public static class EventBusExtensions
{
    public static IServiceCollection AddEventBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register EventBus
        services.AddScoped<IEventBus, EventBus>();

        // Register mappers
        services.AddScoped<IIntegrationEventMapper, IntegrationEventMapper>();

        // Configure MassTransit (covered in Messaging spec)

        return services;
    }
}
```

### Configuration Options

```json
{
  "EventBus": {
    "PublishToMessageBus": true,
    "EnableRetry": true,
    "RetryCount": 3,
    "RetryInterval": "00:00:05"
  }
}
```

## 🔧 Dependencies

### NuGet Packages
- `MediatR` (12.4.1) - In-process messaging
- `MassTransit` (8.2.5) - Distributed messaging abstraction
- `Microsoft.Extensions.Logging` - Logging
- `Microsoft.Extensions.DependencyInjection` - DI

### Project References
- TaskFlow.BuildingBlocks.Common (Domain events)

## 📊 Event Flow Diagram

```
┌─────────────────┐
│  Aggregate      │
│  RaiseDomain    │
│  Event()        │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  EventBus       │
│  PublishAsync() │
└────┬───────┬────┘
     │       │
     │       └──────────────────┐
     │                          │
     ▼                          ▼
┌─────────────┐      ┌──────────────────┐
│  MediatR    │      │  Integration     │
│  (In-Proc)  │      │  Event Mapper    │
└──────┬──────┘      └────────┬─────────┘
       │                      │
       ▼                      ▼
┌─────────────┐      ┌──────────────────┐
│  Domain     │      │  MassTransit     │
│  Event      │      │  (RabbitMQ)      │
│  Handlers   │      └────────┬─────────┘
└─────────────┘               │
                              ▼
                     ┌──────────────────┐
                     │  Other Services  │
                     │  (Consumers)     │
                     └──────────────────┘
```

## ✅ Features

- [x] Abstraction over messaging infrastructure
- [x] Support both in-process and distributed events
- [x] Domain event to integration event mapping
- [x] Batch event publishing
- [x] Async/await support
- [x] Logging and monitoring
- [x] Error handling and retry
- [x] Idempotency support (via EventId)

## 🧪 Testing Strategy

### Unit Tests
- Event publishing logic
- Event mapping logic
- Error handling

### Integration Tests
- End-to-end event flow
- MassTransit integration
- Handler execution

## 📚 References

- [Domain Events Pattern](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [MassTransit Documentation](https://masstransit.io/)

## 🎯 Success Criteria

1. ✅ Events can be published from aggregates
2. ✅ In-process handlers execute successfully
3. ✅ Integration events are published to message bus
4. ✅ Other services can consume integration events
5. ✅ No loss of events (reliability)
6. ✅ Idempotent event handling

---

**Next**: See `Messaging_Specification.md` for MassTransit/RabbitMQ setup
