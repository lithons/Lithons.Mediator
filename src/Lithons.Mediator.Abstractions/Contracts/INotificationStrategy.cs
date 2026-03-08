using Lithons.Mediator.Abstractions.Contexts;

namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Defines the publication strategy used when dispatching a notification through the mediator.
/// </summary>
public interface INotificationStrategy
{
    /// <summary>
    /// Publishes the notification described by <paramref name="context"/> to all registered handlers asynchronously.
    /// </summary>
    /// <param name="context">The context containing the notification, handlers, and supporting services.</param>
    Task PublishAsync(NotificationStrategyContext context);
}
