namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Marker interface for command handler implementations.
/// </summary>
public interface ICommandHandler;

/// <summary>
/// Defines a handler for a command of type <typeparamref name="TCommand"/> that returns <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle.</typeparam>
/// <typeparam name="TResult">The type of the result produced by the handler.</typeparam>
public interface ICommandHandler<in TCommand, TResult> : ICommandHandler
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command and returns a result asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the handler's result.</returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Defines a handler for a command of type <typeparamref name="TCommand"/> that produces no result.
/// </summary>
/// <typeparam name="TCommand">The type of the command to handle.</typeparam>
public interface ICommandHandler<in TCommand> : ICommandHandler
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the specified command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task Handle(TCommand command, CancellationToken cancellationToken);
}
