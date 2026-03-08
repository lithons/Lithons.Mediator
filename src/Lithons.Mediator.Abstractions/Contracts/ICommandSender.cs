namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Defines the ability to send command messages, optionally specifying an execution strategy.
/// </summary>
public interface ICommandSender
{
    /// <summary>
    /// Sends the specified command using the default strategy and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="command">The command message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the handler's result.</returns>
    Task<T> SendAsync<T>(ICommand<T> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the specified command using the provided strategy and returns the result asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <param name="command">The command message to send.</param>
    /// <param name="strategy">The execution strategy to use.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the handler's result.</returns>
    Task<T> SendAsync<T>(ICommand<T> command, ICommandStrategy strategy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the specified void command using the default strategy asynchronously.
    /// </summary>
    /// <param name="command">The command message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the specified void command using the provided strategy, or the default if <see langword="null"/>.
    /// </summary>
    /// <param name="command">The command message to send.</param>
    /// <param name="strategy">The execution strategy to use, or <see langword="null"/> to use the default.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task SendAsync(ICommand command, ICommandStrategy? strategy, CancellationToken cancellationToken = default);
}
