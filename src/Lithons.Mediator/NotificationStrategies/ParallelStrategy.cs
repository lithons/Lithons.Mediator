using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Internal;

namespace Lithons.Mediator.NotificationStrategies;

/// <summary>
/// An <see cref="INotificationStrategy"/> that invokes all registered notification handlers concurrently.
/// </summary>
public class ParallelStrategy : INotificationStrategy
{
    public async Task PublishAsync(NotificationStrategyContext context)
    {
        var notification = context.NotificationContext.Notification;
        var notificationType = notification.GetType();
        var invoker = HandlerInvokerCache.GetNotificationInvoker(notificationType);
        var cancellationToken = context.NotificationContext.CancellationToken;

        var tasks = context.Handlers
            .Select(h => invoker(h, notification, cancellationToken))
            .ToList();

        await Task.WhenAll(tasks);
    }
}
