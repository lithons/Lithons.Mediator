namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Central mediator interface that combines request sending, command sending, and notification publishing.
/// </summary>
public interface IMediator : IRequestSender, ICommandSender, INotificationSender;
