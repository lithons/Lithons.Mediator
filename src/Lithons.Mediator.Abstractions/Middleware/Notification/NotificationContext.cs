using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Abstractions.Middleware.Notification;

/// <summary>
/// Carries the state of a notification as it flows through the notification middleware pipeline.
/// </summary>
public class NotificationContext(
    INotification notification,
    INotificationStrategy notificationStrategy,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken)
{
    /// <summary>
    /// Gets the notification message being published.
    /// </summary>
    public INotification Notification { get; init; } = notification;

    /// <summary>
    /// Gets the strategy used to dispatch the notification to its handlers.
    /// </summary>
    public INotificationStrategy NotificationStrategy { get; init; } = notificationStrategy;

    /// <summary>
    /// Gets the scoped service provider for the current notification.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets a token that can cancel the pipeline execution.
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = cancellationToken;
}
