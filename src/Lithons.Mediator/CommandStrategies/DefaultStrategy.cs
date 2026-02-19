using Lithons.Mediator.Contracts;
using Lithons.Mediator.Contexts;
using Lithons.Mediator.Internal;

namespace Lithons.Mediator.CommandStrategies;

public class DefaultStrategy : ICommandStrategy
{
    public async Task<TResult> ExecuteAsync<TResult>(CommandStrategyContext context)
    {
        var command = context.CommandContext.Command;
        var commandType = command.GetType();
        var resultType = typeof(TResult);

        var invoker = HandlerInvokerCache.GetCommandInvoker(commandType, resultType);
        var task = invoker(context.Handler, command, context.CancellationToken);
        await task.ConfigureAwait(false);

        return (TResult)HandlerInvokerCache.GetResultExtractor(resultType)(task)!;
    }

    public async Task ExecuteAsync(CommandStrategyContext context)
    {
        var command = context.CommandContext.Command;
        var commandType = command.GetType();

        var invoker = HandlerInvokerCache.GetVoidCommandInvoker(commandType);
        await invoker(context.Handler, command, context.CancellationToken).ConfigureAwait(false);
    }
}
