namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Defines the ability to publish notification messages, optionally specifying a publication strategy.
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Publishes the specified notification using the default strategy asynchronously.
    /// </summary>
    /// <param name="notification">The notification message to publish.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendAsync(INotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes the specified notification using the provided strategy, or the default if <see langword="null"/>.
    /// </summary>
    /// <param name="notification">The notification message to publish.</param>
    /// <param name="strategy">The publication strategy to use, or <see langword="null"/> to use the default.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendAsync(INotification notification, INotificationStrategy? strategy, CancellationToken cancellationToken = default);
}
