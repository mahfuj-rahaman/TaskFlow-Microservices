
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.BuildingBlocks.Mediator.Abstractions;
using TaskFlow.BuildingBlocks.Mediator.Adapters;

namespace TaskFlow.BuildingBlocks.Mediator.Extensions;

public static class MediatorExtensions
{
    public static IServiceCollection AddTaskFlowMediator(
        this IServiceCollection services,
        Assembly assembly)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register the dispatcher
        services.AddScoped<IDispatcher, MediatRDispatcher>();

        // Register command and query handlers
        RegisterHandlers(services, assembly, typeof(ICommandHandler<>));
        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));

        // Register MediatR handlers for our custom interfaces
        services.AddTransient(typeof(IRequestHandler<,>), typeof(MediatorCommandHandler<,>));
        services.AddTransient(typeof(IRequestHandler<>), typeof(MediatorCommandHandler<>));
        services.AddTransient(typeof(IRequestHandler<,>), typeof(MediatorQueryHandler<,>));

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly, Type handlerInterface)
    {
        var handlers = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface));

        foreach (var handler in handlers)
        {
            services.AddScoped(handler.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface), handler);
        }
    }
}

// Wrapper classes to adapt MediatR handlers to our custom interfaces

file sealed class MediatorCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>, IRequest<TResponse>
{
    private readonly ICommandHandler<TCommand, TResponse> _handler;

    public MediatorCommandHandler(ICommandHandler<TCommand, TResponse> handler)
    {
        _handler = handler;
    }

    public Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        return _handler.HandleAsync(request, cancellationToken);
    }
}

file sealed class MediatorCommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand, IRequest
{
    private readonly ICommandHandler<TCommand> _handler;

    public MediatorCommandHandler(ICommandHandler<TCommand> handler)
    {
        _handler = handler;
    }

    public Task Handle(TCommand request, CancellationToken cancellationToken)
    {
        return _handler.HandleAsync(request, cancellationToken);
    }
}

file sealed class MediatorQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>, IRequest<TResponse>
{
    private readonly IQueryHandler<TQuery, TResponse> _handler;

    public MediatorQueryHandler(IQueryHandler<TQuery, TResponse> handler)
    {
        _handler = handler;
    }

    public Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken)
    {
        return _handler.HandleAsync(request, cancellationToken);
    }
}
