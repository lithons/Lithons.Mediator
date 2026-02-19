using Lithons.Mediator.Middleware.Notification.Contracts;

namespace Lithons.Mediator.Middleware.Notification;

public class NotificationPipelineBuilder : INotificationPipelineBuilder
{
    private readonly List<Func<NotificationMiddlewareDelegate, NotificationMiddlewareDelegate>> _components = [];

    public NotificationPipelineBuilder(IServiceProvider serviceProvider)
    {
        ApplicationServices = serviceProvider;
    }

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
    public IServiceProvider ApplicationServices { get; set; }

    public INotificationPipelineBuilder Use(Func<NotificationMiddlewareDelegate, NotificationMiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public NotificationMiddlewareDelegate Build()
    {
        NotificationMiddlewareDelegate pipeline = _ => ValueTask.CompletedTask;
        for (var i = _components.Count - 1; i >= 0; i--)
            pipeline = _components[i](pipeline);
        return pipeline;
    }
}
