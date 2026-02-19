using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification;
using Microsoft.Extensions.Logging;

namespace Lithons.Mediator.Abstractions.Contexts;

public record NotificationStrategyContext(
    NotificationContext NotificationContext,
    INotificationHandler[] Handlers,
    ILogger Logger,
    IServiceProvider ServiceProvider,
    CancellationToken CancellationToken = default);
