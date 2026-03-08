namespace Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;

/// <summary>
/// Provides a mechanism to configure the notification middleware pipeline.
/// </summary>
public interface INotificationPipelineBuilder
{
    /// <summary>
    /// Gets a collection of properties shared across the pipeline-building process.
    /// </summary>
    IDictionary<string, object?> Properties { get; }

    /// <summary>
    /// Gets the application-level service provider.
    /// </summary>
    IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// Adds a middleware factory to the pipeline.
    /// </summary>
    /// <param name="middleware">A function that wraps the next delegate with the new middleware component.</param>
    /// <returns>The current <see cref="INotificationPipelineBuilder"/> for chaining.</returns>
    INotificationPipelineBuilder Use(Func<NotificationMiddlewareDelegate, NotificationMiddlewareDelegate> middleware);

    /// <summary>
    /// Builds the composed pipeline delegate from all registered middleware components.
    /// </summary>
    /// <returns>The composed <see cref="NotificationMiddlewareDelegate"/>.</returns>
    NotificationMiddlewareDelegate Build();
}
