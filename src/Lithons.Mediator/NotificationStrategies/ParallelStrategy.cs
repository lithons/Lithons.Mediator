using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Internal;

namespace Lithons.Mediator.NotificationStrategies;

public class ParallelStrategy : INotificationStrategy
{
    public async Task PublishAsync(NotificationStrategyContext context)
    {
        var notification = context.NotificationContext.Notification;
        var notificationType = notification.GetType();
        var invoker = HandlerInvokerCache.GetNotificationInvoker(notificationType);
        var cancellationToken = context.NotificationContext.CancellationToken;

        var tasks = context.Handlers
            .Select(h =>
            {
                try
                {
                    return invoker(h, notification, cancellationToken);
                }
                catch (Exception ex)
                {
                    return Task.FromException(ex);
                }
            })
            .ToList();

        var whenAll = Task.WhenAll(tasks);

        try
        {
            await whenAll;
        }
        catch
        {
            if (whenAll.Exception is not null)
            {
                throw whenAll.Exception;
            }

            // Re-await to surface TaskCanceledException / OperationCanceledException
            await whenAll;
        }
    }
}
