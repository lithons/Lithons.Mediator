namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Marker interface for notification handler implementations.
/// </summary>
public interface INotificationHandler;

/// <summary>
/// Defines a handler for notifications of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the notification to handle.</typeparam>
public interface INotificationHandler<in T> : INotificationHandler
    where T : INotification
{
    /// <summary>
    /// Handles the specified notification asynchronously.
    /// </summary>
    /// <param name="notification">The notification to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task Handle(T notification, CancellationToken cancellationToken);
}
