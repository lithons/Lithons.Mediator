using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Internal;

namespace Lithons.Mediator.Middleware.Command.Components;

public class CommandHandlerInvokerMiddleware(CommandMiddlewareDelegate next) : ICommandMiddleware
{
    public async ValueTask InvokeAsync(CommandContext context)
    {
        var command = context.Command;
        var commandType = command.GetType();
        var strategy = context.CommandStrategy;

        if (context.ResultType is null)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
            var handler = (ICommandHandler)HandlerResolver.ResolveSingle(context.ServiceProvider, handlerType, commandType);
            var strategyContext = new CommandStrategyContext(context, handler, context.ServiceProvider, context.CancellationToken);

            await strategy.ExecuteAsync(strategyContext).ConfigureAwait(false);
        }
        else
        {
            var resultType = context.ResultType;
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
            var handler = (ICommandHandler)HandlerResolver.ResolveSingle(context.ServiceProvider, handlerType, commandType);
            var strategyContext = new CommandStrategyContext(context, handler, context.ServiceProvider, context.CancellationToken);

            var executeDelegate = HandlerInvokerCache.GetStrategyExecuteDelegate(resultType);
            var task = executeDelegate(strategy, strategyContext);
            await task.ConfigureAwait(false);

            context.Result = HandlerInvokerCache.GetResultExtractor(resultType)(task);
        }

        await next(context).ConfigureAwait(false);
    }
}
