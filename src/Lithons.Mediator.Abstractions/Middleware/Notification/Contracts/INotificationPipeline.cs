namespace Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;

/// <summary>
/// Represents the notification middleware pipeline used to dispatch notification messages.
/// </summary>
public interface INotificationPipeline
{
    /// <summary>
    /// Configures the pipeline using the provided builder action and returns the composed delegate.
    /// </summary>
    /// <param name="setup">An action that adds middleware components to the pipeline builder.</param>
    /// <returns>The composed <see cref="NotificationMiddlewareDelegate"/>.</returns>
    NotificationMiddlewareDelegate Setup(Action<INotificationPipelineBuilder> setup);

    /// <summary>
    /// Gets the composed middleware delegate, building the default pipeline on first access.
    /// </summary>
    NotificationMiddlewareDelegate Pipeline { get; }

    /// <summary>
    /// Executes the pipeline for the given context.
    /// </summary>
    /// <param name="context">The context carrying the notification and its execution state.</param>
    Task ExecuteAsync(NotificationContext context);
}
