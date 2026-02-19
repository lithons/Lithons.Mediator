namespace Lithons.Mediator.Contracts;

public interface INotificationSender
{
    Task SendAsync(INotification notification, CancellationToken cancellationToken = default);
    Task SendAsync(INotification notification, INotificationStrategy? strategy, CancellationToken cancellationToken = default);
}
