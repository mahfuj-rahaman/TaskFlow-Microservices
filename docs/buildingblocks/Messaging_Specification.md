# Messaging BuildingBlock Specification

**Project**: TaskFlow.BuildingBlocks.Messaging
**Version**: 1.0.0
**Last Updated**: 2025-11-01
**Status**: Specification

---

## 📋 Overview

The Messaging BuildingBlock provides RabbitMQ integration using MassTransit for reliable, asynchronous communication between microservices.

## 🎯 Purpose

- Configure MassTransit with RabbitMQ
- Provide message broker abstraction
- Handle message routing and exchanges
- Implement retry policies and error handling
- Support request/response patterns
- Provide message serialization/deserialization

## 🏗️ Architecture

```
Service A → MassTransit → RabbitMQ → MassTransit → Service B
            (Publisher)   (Broker)    (Consumer)
```

## 📦 Components

### 1. Configuration Classes

#### MessagingOptions
```csharp
public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public RetryOptions Retry { get; set; } = new();
    public OutboxOptions Outbox { get; set; } = new();
    public bool UseMessageScheduler { get; set; } = true;
}

public sealed class RetryOptions
{
    public int RetryCount { get; set; } = 3;
    public int InitialIntervalSeconds { get; set; } = 5;
    public int IntervalIncrementSeconds { get; set; } = 5;
}

public sealed class OutboxOptions
{
    public bool Enabled { get; set; } = true;
    public int QueryDelay { get; set; } = 100;
    public int QueryLimit { get; set; } = 100;
}
```

### 2. Extension Methods

#### MassTransitExtensions
```csharp
public static class MassTransitExtensions
{
    public static IServiceCollection AddMassTransitMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var options = configuration
            .GetSection(MessagingOptions.SectionName)
            .Get<MessagingOptions>() ?? new MessagingOptions();

        services.AddMassTransit(x =>
        {
            // Register consumers
            configureConsumers?.Invoke(x);

            // Configure RabbitMQ
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(options.Host, options.Port, options.VirtualHost, h =>
                {
                    h.Username(options.Username);
                    h.Password(options.Password);
                });

                // Configure retry
                cfg.UseMessageRetry(r => r.Incremental(
                    options.Retry.RetryCount,
                    TimeSpan.FromSeconds(options.Retry.InitialIntervalSeconds),
                    TimeSpan.FromSeconds(options.Retry.IntervalIncrementSeconds)));

                // Configure endpoints
                cfg.ConfigureEndpoints(context);
            });

            // Message scheduler (for delayed/scheduled messages)
            if (options.UseMessageScheduler)
            {
                x.AddDelayedMessageScheduler();
            }
        });

        return services;
    }
}
```

### 3. Message Contracts

#### Base Message Contract
```csharp
public interface IMessage
{
    Guid MessageId { get; }
    DateTime Timestamp { get; }
}

public abstract record MessageBase : IMessage
{
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
```

#### Command Messages
```csharp
// Example: Create Order Command
public sealed record CreateOrderCommand : MessageBase
{
    public Guid CustomerId { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
    public decimal TotalAmount { get; init; }
}
```

#### Event Messages
```csharp
// Example: Order Created Event
public sealed record OrderCreatedEvent : MessageBase
{
    public Guid OrderId { get; init; }
    public Guid CustomerId { get; init; }
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

### 4. Consumer Base Classes

#### BaseConsumer<TMessage>
```csharp
public abstract class BaseConsumer<TMessage> : IConsumer<TMessage>
    where TMessage : class, IMessage
{
    protected readonly ILogger Logger;

    protected BaseConsumer(ILogger logger)
    {
        Logger = logger;
    }

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        Logger.LogInformation(
            "Consuming message {MessageType} with ID {MessageId}",
            typeof(TMessage).Name,
            context.Message.MessageId);

        try
        {
            await ConsumeMessage(context);

            Logger.LogInformation(
                "Successfully consumed message {MessageType} with ID {MessageId}",
                typeof(TMessage).Name,
                context.Message.MessageId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Error consuming message {MessageType} with ID {MessageId}",
                typeof(TMessage).Name,
                context.Message.MessageId);

            throw;
        }
    }

    protected abstract Task ConsumeMessage(ConsumeContext<TMessage> context);
}
```

## 📝 Usage Examples

### 1. Publishing Messages

```csharp
// In a Command Handler
public sealed class CreateOrderCommandHandler
{
    private readonly IPublishEndpoint _publishEndpoint;

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var order = OrderEntity.Create(request.CustomerId, request.Items);
        await _repository.AddAsync(order, ct);

        // Publish integration event
        await _publishEndpoint.Publish(new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        }, ct);

        return Result.Success(order.Id);
    }
}
```

### 2. Consuming Messages

```csharp
// Consumer in another service (e.g., Notification Service)
public sealed class OrderCreatedEventConsumer : BaseConsumer<OrderCreatedEvent>
{
    private readonly INotificationService _notificationService;

    public OrderCreatedEventConsumer(
        INotificationService notificationService,
        ILogger<OrderCreatedEventConsumer> logger)
        : base(logger)
    {
        _notificationService = notificationService;
    }

    protected override async Task ConsumeMessage(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        await _notificationService.SendOrderConfirmationAsync(
            message.CustomerId,
            message.OrderId,
            message.TotalAmount);
    }
}
```

### 3. Request/Response Pattern

```csharp
// Request
public sealed record GetUserDetailsRequest : MessageBase
{
    public Guid UserId { get; init; }
}

// Response
public sealed record GetUserDetailsResponse : MessageBase
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}

// Consumer (responds to request)
public sealed class GetUserDetailsConsumer : IConsumer<GetUserDetailsRequest>
{
    private readonly IUserRepository _userRepository;

    public async Task Consume(ConsumeContext<GetUserDetailsRequest> context)
    {
        var user = await _userRepository.GetByIdAsync(context.Message.UserId);

        await context.RespondAsync(new GetUserDetailsResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}"
        });
    }
}

// Client (sends request and waits for response)
public sealed class OrderService
{
    private readonly IRequestClient<GetUserDetailsRequest> _client;

    public async Task<UserDetails> GetUserDetailsAsync(Guid userId, CancellationToken ct)
    {
        var response = await _client.GetResponse<GetUserDetailsResponse>(
            new GetUserDetailsRequest { UserId = userId },
            ct);

        return new UserDetails
        {
            UserId = response.Message.UserId,
            Email = response.Message.Email,
            FullName = response.Message.FullName
        };
    }
}
```

## ⚙️ Service Registration

### In Program.cs (Each Service)

```csharp
// Order Service
builder.Services.AddMassTransitMessaging(
    builder.Configuration,
    cfg =>
    {
        // Register consumers for this service
        cfg.AddConsumer<PaymentProcessedEventConsumer>();
        cfg.AddConsumer<InventoryReservedEventConsumer>();
    });

// Notification Service
builder.Services.AddMassTransitMessaging(
    builder.Configuration,
    cfg =>
    {
        // Register consumers for this service
        cfg.AddConsumer<OrderCreatedEventConsumer>();
        cfg.AddConsumer<UserRegisteredEventConsumer>();
    });
```

## ⚙️ Configuration

### appsettings.json

```json
{
  "Messaging": {
    "Host": "localhost",
    "Port": 5672,
    "VirtualHost": "/",
    "Username": "guest",
    "Password": "guest",
    "Retry": {
      "RetryCount": 3,
      "InitialIntervalSeconds": 5,
      "IntervalIncrementSeconds": 5
    },
    "Outbox": {
      "Enabled": true,
      "QueryDelay": 100,
      "QueryLimit": 100
    },
    "UseMessageScheduler": true
  }
}
```

### appsettings.Production.json

```json
{
  "Messaging": {
    "Host": "rabbitmq-prod.taskflow.com",
    "Port": 5672,
    "VirtualHost": "/taskflow",
    "Username": "${RABBITMQ_USERNAME}",
    "Password": "${RABBITMQ_PASSWORD}"
  }
}
```

## 🔧 Dependencies

### NuGet Packages
- `MassTransit` (8.2.5)
- `MassTransit.RabbitMQ` (8.2.5)
- `Microsoft.Extensions.Configuration` (8.0.0)
- `Microsoft.Extensions.DependencyInjection` (8.0.0)
- `Microsoft.Extensions.Logging` (8.0.0)

## 📊 Message Flow

```
┌─────────────────┐
│  Order Service  │
│  (Publisher)    │
└────────┬────────┘
         │ Publish OrderCreatedEvent
         ▼
┌─────────────────┐
│   RabbitMQ      │
│   Exchange      │
└────┬───────┬────┘
     │       │
     │       └──────────────────────┐
     │                              │
     ▼                              ▼
┌──────────────────┐      ┌──────────────────┐
│  Notification    │      │  Inventory       │
│  Service         │      │  Service         │
│  (Consumer)      │      │  (Consumer)      │
└──────────────────┘      └──────────────────┘
```

## ✅ Features

- [x] RabbitMQ integration via MassTransit
- [x] Automatic retry with exponential backoff
- [x] Dead letter queue handling
- [x] Message serialization (JSON)
- [x] Request/Response pattern support
- [x] Scheduled/Delayed messages
- [x] Outbox pattern support
- [x] Consumer lifecycle management
- [x] Health checks
- [x] Observability (metrics, tracing)

## 🧪 Testing Strategy

### Unit Tests
- Message serialization/deserialization
- Consumer logic
- Retry policies

### Integration Tests
- End-to-end message flow
- RabbitMQ connectivity
- Multiple consumers
- Error scenarios

### Test Harness
```csharp
[Fact]
public async Task OrderCreatedEvent_ShouldBeConsumed()
{
    var harness = new InMemoryTestHarness();
    var consumerHarness = harness.Consumer<OrderCreatedEventConsumer>();

    await harness.Start();

    try
    {
        await harness.InputQueueSendEndpoint.Send(new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m
        });

        Assert.True(await harness.Consumed.Any<OrderCreatedEvent>());
        Assert.True(await consumerHarness.Consumed.Any<OrderCreatedEvent>());
    }
    finally
    {
        await harness.Stop();
    }
}
```

## 🚨 Error Handling

### Retry Strategy
1. **Immediate Retry**: 3 attempts with incremental backoff (5s, 10s, 15s)
2. **Dead Letter Queue**: After exhausting retries, move to DLQ
3. **Circuit Breaker**: Prevent cascading failures

### Poison Messages
- Messages that consistently fail are moved to `_skipped` queue
- Manual intervention required for poison messages
- Monitoring alerts configured for DLQ growth

## 📈 Monitoring

### Metrics to Track
- Message publish rate
- Message consumption rate
- Consumer lag
- Error rate
- Retry rate
- DLQ message count

### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddRabbitMQ(rabbitConnectionString, name: "rabbitmq");
```

## 📚 References

- [MassTransit Documentation](https://masstransit.io/)
- [RabbitMQ Documentation](https://www.rabbitmq.com/documentation.html)
- [Microservices Messaging Patterns](https://microservices.io/patterns/communication-style/messaging.html)

## 🎯 Success Criteria

1. ✅ Messages are reliably delivered between services
2. ✅ Failed messages are retried automatically
3. ✅ Dead letter queue captures permanently failed messages
4. ✅ Consumers process messages idempotently
5. ✅ System handles high message throughput
6. ✅ Monitoring provides visibility into message flow

---

**Next**: See `Caching_Specification.md` for Redis caching setup
