namespace Lithons.Mediator.Contracts;

public interface IMediator : IRequestSender, ICommandSender, INotificationSender;
