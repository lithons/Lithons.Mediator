using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Request;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
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
        await _requestPipeline.ExecuteAsync(context);
        return (T)context.Response;
    }

    public Task<T> SendAsync<T>(ICommand<T> command, CancellationToken cancellationToken = default)
        => SendAsync(command, _defaultCommandStrategy, cancellationToken);

    public async Task<T> SendAsync<T>(ICommand<T> command, ICommandStrategy strategy, CancellationToken cancellationToken = default)
    {
        var context = new CommandContext(command, strategy, typeof(T), _serviceProvider, cancellationToken);
        await _commandPipeline.InvokeAsync(context);
        return (T)context.Result!;
    }

    public Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
        => SendAsync(command, _defaultCommandStrategy, cancellationToken);

    public async Task SendAsync(ICommand command, ICommandStrategy? strategy, CancellationToken cancellationToken = default)
    {
        strategy ??= _defaultCommandStrategy;
        var context = new CommandContext(command, strategy, null, _serviceProvider, cancellationToken);
        await _commandPipeline.InvokeAsync(context);
    }

    public Task SendAsync(INotification notification, CancellationToken cancellationToken = default)
        => SendAsync(notification, _defaultNotificationStrategy, cancellationToken);

    public async Task SendAsync(INotification notification, INotificationStrategy? strategy, CancellationToken cancellationToken = default)
    {
        strategy ??= _defaultNotificationStrategy;
        var context = new NotificationContext(notification, strategy, _serviceProvider, cancellationToken);
        await _notificationPipeline.ExecuteAsync(context);
    }
}
