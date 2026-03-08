namespace Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;

/// <summary>
/// Defines a middleware component in the notification pipeline.
/// </summary>
public interface INotificationMiddleware
{
    /// <summary>
    /// Processes the current <paramref name="context"/> and optionally invokes the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The context for the current notification.</param>
    ValueTask InvokeAsync(NotificationContext context);
}
