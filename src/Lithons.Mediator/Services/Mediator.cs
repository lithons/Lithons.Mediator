using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Request;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Lithons.Mediator.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Lithons.Mediator.Services;

public class Mediator : IMediator
{
    private readonly IRequestPipeline _requestPipeline;
    private readonly ICommandPipeline _commandPipeline;
    private readonly INotificationPipeline _notificationPipeline;
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationStrategy _defaultNotificationStrategy;
    private readonly ICommandStrategy _defaultCommandStrategy;

    public Mediator(
        IRequestPipeline requestPipeline,
        ICommandPipeline commandPipeline,
        INotificationPipeline notificationPipeline,
        IOptions<Options.MediatorOptions> options,
        IServiceProvider serviceProvider)
    {
        _requestPipeline = requestPipeline;
        _commandPipeline = commandPipeline;
        _notificationPipeline = notificationPipeline;
        _serviceProvider = serviceProvider;
        _defaultNotificationStrategy = options.Value.DefaultNotificationStrategy;
        _defaultCommandStrategy = options.Value.DefaultCommandStrategy;
    }

    public async Task<T> SendAsync<T>(IRequest<T> request, CancellationToken cancellationToken = default)
    {
        var context = new RequestContext(request, typeof(T), _serviceProvider, cancellationToken);
        try
        {
            await _requestPipeline.ExecuteAsync(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!await TryHandleExceptionAsync(ex, request, cancellationToken))
                throw;
            return default!;
        }
        return (T)context.Response;
    }

    public Task<T> SendAsync<T>(ICommand<T> command, CancellationToken cancellationToken = default)
        => SendAsync(command, _defaultCommandStrategy, cancellationToken);

    public async Task<T> SendAsync<T>(ICommand<T> command, ICommandStrategy strategy, CancellationToken cancellationToken = default)
    {
        var context = new CommandContext(command, strategy, typeof(T), _serviceProvider, cancellationToken);
        try
        {
            await _commandPipeline.InvokeAsync(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!await TryHandleExceptionAsync(ex, command, cancellationToken))
                throw;
            return default!;
        }
        return (T)context.Result!;
    }

    public Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
        => SendAsync(command, _defaultCommandStrategy, cancellationToken);

    public async Task SendAsync(ICommand command, ICommandStrategy? strategy, CancellationToken cancellationToken = default)
    {
        strategy ??= _defaultCommandStrategy;
        var context = new CommandContext(command, strategy, null, _serviceProvider, cancellationToken);
        try
        {
            await _commandPipeline.InvokeAsync(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!await TryHandleExceptionAsync(ex, command, cancellationToken))
                throw;
        }
    }

    public Task SendAsync(INotification notification, CancellationToken cancellationToken = default)
        => SendAsync(notification, _defaultNotificationStrategy, cancellationToken);

    public async Task SendAsync(INotification notification, INotificationStrategy? strategy, CancellationToken cancellationToken = default)
    {
        strategy ??= _defaultNotificationStrategy;
        var context = new NotificationContext(notification, strategy, _serviceProvider, cancellationToken);
        try
        {
            await _notificationPipeline.ExecuteAsync(context);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            if (!await TryHandleExceptionAsync(ex, notification, cancellationToken))
                throw;
        }
    }

    private async ValueTask<bool> TryHandleExceptionAsync(Exception exception, object message, CancellationToken cancellationToken)
    {
        var messageType = message.GetType();
        var typedHandlerType = typeof(IExceptionHandler<>).MakeGenericType(messageType);
        var typedHandler = _serviceProvider.GetService(typedHandlerType);

        if (typedHandler is not null)
        {
            var invoker = ExceptionHandlerInvokerCache.GetInvoker(messageType);
            if (await invoker(typedHandler, exception, message, cancellationToken))
                return true;
        }

        var globalHandler = _serviceProvider.GetService<IExceptionHandler>();
        if (globalHandler is not null && await globalHandler.Handle(exception, message, cancellationToken))
            return true;

        return false;
    }
}
