using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.CommandStrategies;

/// <summary>
/// An <see cref="ICommandStrategy"/> that enqueues the command onto the background channel for deferred processing.
/// </summary>
/// <remarks>Requires background command processing to be registered via <c>AddBackgroundCommandProcessing</c>.
/// Return values are not available when using this strategy.</remarks>
public class BackgroundStrategy : ICommandStrategy
{
    public async Task<TResult> ExecuteAsync<TResult>(CommandStrategyContext context)
    {
        var channel = context.ServiceProvider.GetRequiredService<ICommandsChannel>();
        await channel.Writer.WriteAsync(context.CommandContext, context.CancellationToken);
        return default!;
    }

    public async Task ExecuteAsync(CommandStrategyContext context)
    {
        var channel = context.ServiceProvider.GetRequiredService<ICommandsChannel>();
        await channel.Writer.WriteAsync(context.CommandContext, context.CancellationToken);
    }
}
