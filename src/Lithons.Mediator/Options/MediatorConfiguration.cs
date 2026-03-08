using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
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

    public MediatorConfiguration AddRequestHandler(Type handlerType)
    {
        _services.AddRequestHandler(handlerType);
        return this;
    }

    public MediatorConfiguration AddCommandHandler(Type handlerType)
    {
        _services.AddCommandHandler(handlerType);
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
        _services.AddExceptionHandler<THandler>();
        return this;
    }

    public MediatorConfiguration AddExceptionHandler<TMessage, THandler>()
        where THandler : class, IExceptionHandler<TMessage>
    {
        _services.AddExceptionHandler<TMessage, THandler>();
        return this;
    }
}
