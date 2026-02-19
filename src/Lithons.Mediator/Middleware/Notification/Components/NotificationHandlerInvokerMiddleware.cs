using Lithons.Mediator.Contracts;
using Lithons.Mediator.Contexts;
using Lithons.Mediator.Middleware.Notification.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lithons.Mediator.Middleware.Notification.Components;

public class NotificationHandlerInvokerMiddleware(
    NotificationMiddlewareDelegate next,
    ILogger<NotificationHandlerInvokerMiddleware> logger) : INotificationMiddleware
{
    public async ValueTask InvokeAsync(NotificationContext context)
    {
        var notification = context.Notification;
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var handlers = context.ServiceProvider.GetServices(handlerType)
            .Cast<INotificationHandler>()
            .ToArray();

        var strategyContext = new NotificationStrategyContext(
            context, handlers, logger, context.ServiceProvider, context.CancellationToken);

        await context.NotificationStrategy.PublishAsync(strategyContext);
        await next(context);
    }
}
