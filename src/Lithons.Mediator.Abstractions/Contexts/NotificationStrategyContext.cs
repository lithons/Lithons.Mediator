using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification;
using Microsoft.Extensions.Logging;

namespace Lithons.Mediator.Abstractions.Contexts;

/// <summary>
/// Provides contextual data passed to an <see cref="INotificationStrategy"/> when publishing a notification.
/// </summary>
/// <param name="NotificationContext">The notification pipeline context carrying the notification and its execution state.</param>
/// <param name="Handlers">The resolved handlers that will receive the notification.</param>
/// <param name="Logger">The logger available for use by the strategy.</param>
/// <param name="ServiceProvider">The scoped service provider for the current execution.</param>
/// <param name="CancellationToken">A token to cancel the operation.</param>
public record NotificationStrategyContext(
    NotificationContext NotificationContext,
    INotificationHandler[] Handlers,
    ILogger Logger,
    IServiceProvider ServiceProvider,
    CancellationToken CancellationToken = default);
