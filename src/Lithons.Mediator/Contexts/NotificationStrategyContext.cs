using Lithons.Mediator.Contracts;
using Lithons.Mediator.Middleware.Notification;
using Microsoft.Extensions.Logging;

namespace Lithons.Mediator.Contexts;

public record NotificationStrategyContext(
    NotificationContext NotificationContext,
    INotificationHandler[] Handlers,
    ILogger Logger,
    IServiceProvider ServiceProvider,
    CancellationToken CancellationToken = default);
