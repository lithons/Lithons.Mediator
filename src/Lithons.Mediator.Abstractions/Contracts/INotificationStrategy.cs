using Lithons.Mediator.Abstractions.Contexts;

namespace Lithons.Mediator.Abstractions.Contracts;

public interface INotificationStrategy
{
    Task PublishAsync(NotificationStrategyContext context);
}
