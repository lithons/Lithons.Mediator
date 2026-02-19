namespace Lithons.Mediator.Abstractions.Contracts;

public interface IMediator : IRequestSender, ICommandSender, INotificationSender;
