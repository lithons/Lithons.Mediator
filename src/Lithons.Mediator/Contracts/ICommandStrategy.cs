using Lithons.Mediator.Contexts;

namespace Lithons.Mediator.Contracts;

public interface ICommandStrategy
{
    Task<TResult> ExecuteAsync<TResult>(CommandStrategyContext context);
    Task ExecuteAsync(CommandStrategyContext context);
}
