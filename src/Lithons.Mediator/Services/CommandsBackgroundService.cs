using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Lithons.Mediator.Services;

/// <summary>
/// Hosted service that reads commands from the <see cref="ICommandsChannel"/> and processes them in the background.
/// </summary>
public sealed class CommandsBackgroundService(
    ICommandsChannel channel,
    IServiceScopeFactory scopeFactory,
    ILogger<CommandsBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var commandContext in channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                await ProcessCommandAsync(commandContext, scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to process background command {CommandType}.",
                    commandContext.Command.GetType().Name);
            }
        }
    }

    private static async Task ProcessCommandAsync(
        CommandContext commandContext,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var command = commandContext.Command;
        var commandType = command.GetType();

        if (commandContext.ResultType is null)
        {
            var handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);
            var handler = (ICommandHandler)HandlerResolver.ResolveSingle(serviceProvider, handlerType, commandType);
            var invoker = HandlerInvokerCache.GetVoidCommandInvoker(commandType);
            await invoker(handler, command, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            var resultType = commandContext.ResultType;
            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
            var handler = (ICommandHandler)HandlerResolver.ResolveSingle(serviceProvider, handlerType, commandType);
            var invoker = HandlerInvokerCache.GetCommandInvoker(commandType, resultType);
            await invoker(handler, command, cancellationToken).ConfigureAwait(false);
        }
    }
}
