using Lithons.Mediator.Abstractions.Contexts;

namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Defines the execution strategy used when dispatching a command through the mediator.
/// </summary>
public interface ICommandStrategy
{
    /// <summary>
    /// Executes the command described by <paramref name="context"/> and returns a result asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by the handler.</typeparam>
    /// <param name="context">The context containing the command, handler, and supporting services.</param>
    Task<TResult> ExecuteAsync<TResult>(CommandStrategyContext context);

    /// <summary>
    /// Executes the command described by <paramref name="context"/> without returning a result asynchronously.
    /// </summary>
    /// <param name="context">The context containing the command, handler, and supporting services.</param>
    Task ExecuteAsync(CommandStrategyContext context);
}
