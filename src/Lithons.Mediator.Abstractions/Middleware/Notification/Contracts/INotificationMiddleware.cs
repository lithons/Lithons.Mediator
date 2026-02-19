namespace Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;

public interface INotificationMiddleware
{
    ValueTask InvokeAsync(NotificationContext context);
}
