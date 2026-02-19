using Lithons.Mediator.Contexts;

namespace Lithons.Mediator.Contracts;

public interface INotificationStrategy
{
    Task PublishAsync(NotificationStrategyContext context);
}
