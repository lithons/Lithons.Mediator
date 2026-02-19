namespace Lithons.Mediator.Abstractions.Contracts;

public interface ICommandSender
{
    Task<T> SendAsync<T>(ICommand<T> command, CancellationToken cancellationToken = default);
    Task<T> SendAsync<T>(ICommand<T> command, ICommandStrategy strategy, CancellationToken cancellationToken = default);
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);
    Task SendAsync(ICommand command, ICommandStrategy? strategy, CancellationToken cancellationToken = default);
}
