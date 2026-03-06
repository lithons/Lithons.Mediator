namespace Lithons.Mediator.Abstractions.Contracts;

public interface INotificationHandler;

public interface INotificationHandler<in T> : INotificationHandler
    where T : INotification
{
    Task Handle(T notification, CancellationToken cancellationToken);
}
