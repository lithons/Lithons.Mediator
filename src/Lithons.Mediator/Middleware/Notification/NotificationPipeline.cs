using Lithons.Mediator.Abstractions.Middleware.Notification;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;

namespace Lithons.Mediator.Middleware.Notification;

/// <summary>
/// Default implementation of <see cref="INotificationPipeline"/>.
/// </summary>
public class NotificationPipeline : INotificationPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private NotificationMiddlewareDelegate? _pipeline;

    /// <summary>
    /// Initializes a new instance of <see cref="NotificationPipeline"/>.
    /// </summary>
    /// <param name="serviceProvider">The application-level service provider.</param>
    public NotificationPipeline(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public NotificationMiddlewareDelegate Pipeline => _pipeline ??= Setup(x => x.UseNotificationHandlers());

    /// <inheritdoc />
    public NotificationMiddlewareDelegate Setup(Action<INotificationPipelineBuilder>? setup = default)
    {
        var builder = new NotificationPipelineBuilder(_serviceProvider);
        setup?.Invoke(builder);
        _pipeline = builder.Build();
        return _pipeline;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(NotificationContext context) => await Pipeline(context);
}
