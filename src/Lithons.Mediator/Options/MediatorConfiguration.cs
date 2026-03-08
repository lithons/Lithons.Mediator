using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using System.Threading.Channels;

namespace Lithons.Mediator.Options;

/// <summary>
/// Fluent builder used to configure the mediator during application startup.
/// </summary>
public sealed class MediatorConfiguration
{
    private readonly IServiceCollection _services;

    internal MediatorConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Gets or sets the default strategy used when publishing notifications.
    /// </summary>
    public INotificationStrategy DefaultNotificationStrategy { get; set; } = NotificationStrategy.Sequential;

    /// <summary>
    /// Gets or sets the default strategy used when sending commands.
    /// </summary>
    public ICommandStrategy DefaultCommandStrategy { get; set; } = CommandStrategy.Default;

    /// <summary>
    /// Scans the specified assembly and registers all discovered handlers.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <param name="filter">An optional predicate to filter which handler types are registered.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddHandlersFromAssembly(Assembly assembly, Func<Type, bool>? filter = null)
    {
        _services.AddHandlersFromAssembly(assembly, filter);
        return this;
    }

    /// <summary>
    /// Scans the assembly containing <typeparamref name="T"/> and registers all discovered handlers.
    /// </summary>
    /// <typeparam name="T">A type whose containing assembly is scanned.</typeparam>
    /// <param name="filter">An optional predicate to filter which handler types are registered.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddHandlersFromAssembly<T>(Func<Type, bool>? filter = null)
    {
        _services.AddHandlersFromAssemblyContaining<T>(filter);
        return this;
    }

    /// <summary>
    /// Registers the specified request handler type.
    /// </summary>
    /// <typeparam name="THandler">The request handler type to register.</typeparam>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddRequestHandler<THandler>()
        where THandler : class, IRequestHandler
    {
        _services.AddRequestHandler<THandler>();
        return this;
    }

    /// <summary>
    /// Registers the specified request handler type.
    /// </summary>
    /// <param name="handlerType">The request handler type to register.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddRequestHandler(Type handlerType)
    {
        _services.AddRequestHandler(handlerType);
        return this;
    }

    /// <summary>
    /// Registers the specified command handler type.
    /// </summary>
    /// <typeparam name="THandler">The command handler type to register.</typeparam>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddCommandHandler<THandler>()
        where THandler : class, ICommandHandler
    {
        _services.AddCommandHandler<THandler>();
        return this;
    }

    /// <summary>
    /// Registers the specified command handler type.
    /// </summary>
    /// <param name="handlerType">The command handler type to register.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddCommandHandler(Type handlerType)
    {
        _services.AddCommandHandler(handlerType);
        return this;
    }

    /// <summary>
    /// Registers the specified notification handler type.
    /// </summary>
    /// <typeparam name="THandler">The notification handler type to register.</typeparam>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddNotificationHandler<THandler>()
        where THandler : class, INotificationHandler
    {
        _services.AddNotificationHandler<THandler>();
        return this;
    }

    /// <summary>
    /// Registers the specified notification handler type.
    /// </summary>
    /// <param name="handlerType">The notification handler type to register.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddNotificationHandler(Type handlerType)
    {
        _services.AddNotificationHandler(handlerType);
        return this;
    }

    /// <summary>
    /// Registers a global exception handler that is invoked for any unhandled exception in the mediator pipeline.
    /// </summary>
    /// <typeparam name="THandler">The exception handler type to register.</typeparam>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddExceptionHandler<THandler>()
        where THandler : class, IExceptionHandler
    {
        _services.TryAddScoped<IExceptionHandler, THandler>();
        return this;
    }

    /// <summary>
    /// Registers a typed exception handler invoked when an exception occurs while processing <typeparamref name="TMessage"/>.
    /// </summary>
    /// <typeparam name="TMessage">The message type whose exceptions this handler covers.</typeparam>
    /// <typeparam name="THandler">The exception handler type to register.</typeparam>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddExceptionHandler<TMessage, THandler>()
        where THandler : class, IExceptionHandler<TMessage>
    {
        _services.TryAddScoped<IExceptionHandler<TMessage>, THandler>();
        return this;
    }

    /// <summary>
    /// Registers the hosted background service and channel required for background command processing.
    /// </summary>
    /// <param name="configureChannel">An optional action to configure the underlying <see cref="BoundedChannelOptions"/>.
    /// When <see langword="null"/>, an unbounded channel is used.</param>
    /// <returns>The current <see cref="MediatorConfiguration"/> for chaining.</returns>
    public MediatorConfiguration AddBackgroundCommandProcessing(
        Action<BoundedChannelOptions>? configureChannel = null)
    {
        _services.AddBackgroundCommandProcessing(configureChannel);
        return this;
    }
}
