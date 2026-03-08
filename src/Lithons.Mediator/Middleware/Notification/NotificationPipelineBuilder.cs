using Lithons.Mediator.Abstractions.Middleware.Notification;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;

namespace Lithons.Mediator.Middleware.Notification;

/// <summary>
/// Default implementation of <see cref="INotificationPipelineBuilder"/>.
/// </summary>
public class NotificationPipelineBuilder : INotificationPipelineBuilder
{
    private readonly List<Func<NotificationMiddlewareDelegate, NotificationMiddlewareDelegate>> _components = [];

    /// <summary>
    /// Initializes a new instance of <see cref="NotificationPipelineBuilder"/>.
    /// </summary>
    /// <param name="serviceProvider">The application-level service provider.</param>
    public NotificationPipelineBuilder(IServiceProvider serviceProvider)
    {
        ApplicationServices = serviceProvider;
    }

    /// <inheritdoc />
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    /// <inheritdoc />
    public IServiceProvider ApplicationServices { get; set; }

    /// <inheritdoc />
    public INotificationPipelineBuilder Use(Func<NotificationMiddlewareDelegate, NotificationMiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    /// <inheritdoc />
    public NotificationMiddlewareDelegate Build()
    {
        NotificationMiddlewareDelegate pipeline = _ => ValueTask.CompletedTask;
        for (var i = _components.Count - 1; i >= 0; i--)
            pipeline = _components[i](pipeline);
        return pipeline;
    }
}
