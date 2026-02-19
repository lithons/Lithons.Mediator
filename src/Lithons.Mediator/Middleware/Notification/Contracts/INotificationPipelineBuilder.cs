namespace Lithons.Mediator.Middleware.Notification.Contracts;

public interface INotificationPipelineBuilder
{
    IDictionary<string, object?> Properties { get; }
    IServiceProvider ApplicationServices { get; }
    INotificationPipelineBuilder Use(Func<NotificationMiddlewareDelegate, NotificationMiddlewareDelegate> middleware);
    NotificationMiddlewareDelegate Build();
}
