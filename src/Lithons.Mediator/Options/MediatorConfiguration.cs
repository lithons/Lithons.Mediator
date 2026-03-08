using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Lithons.Mediator.Options;

public sealed class MediatorConfiguration
{
    private readonly IServiceCollection _services;

    internal MediatorConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    public INotificationStrategy DefaultNotificationStrategy { get; set; } = NotificationStrategy.Sequential;
    public ICommandStrategy DefaultCommandStrategy { get; set; } = CommandStrategy.Default;

    public MediatorConfiguration AddHandlersFromAssembly(Assembly assembly, Func<Type, bool>? filter = null)
    {
        _services.AddHandlersFromAssembly(assembly, filter);
        return this;
    }

    public MediatorConfiguration AddHandlersFromAssembly<T>(Func<Type, bool>? filter = null)
    {
        _services.AddHandlersFromAssemblyContaining<T>(filter);
        return this;
    }

    public MediatorConfiguration AddRequestHandler<THandler>()
        where THandler : class, IRequestHandler
    {
        _services.AddRequestHandler<THandler>();
        return this;
    }

    public MediatorConfiguration AddRequestHandler(Type handlerType)
    {
        _services.AddRequestHandler(handlerType);
        return this;
    }

    public MediatorConfiguration AddCommandHandler<THandler>()
        where THandler : class, ICommandHandler
    {
        _services.AddCommandHandler<THandler>();
        return this;
    }

    public MediatorConfiguration AddCommandHandler(Type handlerType)
    {
        _services.AddCommandHandler(handlerType);
        return this;
    }

    public MediatorConfiguration AddNotificationHandler<THandler>()
        where THandler : class, INotificationHandler
    {
        _services.AddNotificationHandler<THandler>();
        return this;
    }

    public MediatorConfiguration AddNotificationHandler(Type handlerType)
    {
        _services.AddNotificationHandler(handlerType);
        return this;
    }

    public MediatorConfiguration AddExceptionHandler<THandler>()
        where THandler : class, IExceptionHandler
    {
        _services.TryAddScoped<IExceptionHandler, THandler>();
        return this;
    }

    public MediatorConfiguration AddExceptionHandler<TMessage, THandler>()
        where THandler : class, IExceptionHandler<TMessage>
    {
        _services.TryAddScoped<IExceptionHandler<TMessage>, THandler>();
        return this;
    }
}
