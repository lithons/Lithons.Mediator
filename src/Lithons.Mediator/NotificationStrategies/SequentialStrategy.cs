using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Internal;

namespace Lithons.Mediator.NotificationStrategies;

public class SequentialStrategy : INotificationStrategy
{
    public async Task PublishAsync(NotificationStrategyContext context)
    {
        var notification = context.NotificationContext.Notification;
        var notificationType = notification.GetType();
        var invoker = HandlerInvokerCache.GetNotificationInvoker(notificationType);
        var cancellationToken = context.NotificationContext.CancellationToken;

        foreach (var handler in context.Handlers)
        {
            await invoker(handler, notification, cancellationToken).ConfigureAwait(false);
        }
    }
}
