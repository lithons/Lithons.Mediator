using Lithons.Mediator.Contracts;

namespace Lithons.Mediator.Middleware.Notification;

public class NotificationContext(
    INotification notification,
    INotificationStrategy notificationStrategy,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken)
{
    public INotification Notification { get; init; } = notification;
    public INotificationStrategy NotificationStrategy { get; init; } = notificationStrategy;
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public CancellationToken CancellationToken { get; init; } = cancellationToken;
}
