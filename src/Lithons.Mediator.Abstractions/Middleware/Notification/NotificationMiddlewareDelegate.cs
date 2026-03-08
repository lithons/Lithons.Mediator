namespace Lithons.Mediator.Abstractions.Middleware.Notification;

/// <summary>
/// Represents a step in the notification middleware pipeline.
/// </summary>
/// <param name="context">The <see cref="NotificationContext"/> for the current notification.</param>
public delegate ValueTask NotificationMiddlewareDelegate(NotificationContext context);
