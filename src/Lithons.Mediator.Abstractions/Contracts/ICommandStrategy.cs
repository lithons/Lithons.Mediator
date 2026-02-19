using Lithons.Mediator.Abstractions.Contexts;

namespace Lithons.Mediator.Abstractions.Contracts;

public interface ICommandStrategy
{
    Task<TResult> ExecuteAsync<TResult>(CommandStrategyContext context);
    Task ExecuteAsync(CommandStrategyContext context);
}
