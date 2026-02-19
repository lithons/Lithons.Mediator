using Lithons.Mediator.Contracts;
using Lithons.Mediator.Contexts;
using Microsoft.Extensions.DependencyInjection;

namespace Lithons.Mediator.CommandStrategies;

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
