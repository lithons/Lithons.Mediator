using Lithons.Mediator.Abstractions.Contexts;
using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using System.Threading.Channels;

namespace Lithons.Mediator.Tests.Strategies;

public class CommandStrategyTests
{
    private record SumCommand(int A, int B) : ICommand<int>;

    private class SumCommandHandler : ICommandHandler<SumCommand, int>
    {
        public Task<int> HandleAsync(SumCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.A + command.B);
    }

    private record VoidCommand : ICommand;

    private class VoidCommandHandler : ICommandHandler<VoidCommand>
    {
        public bool Called { get; private set; }

        public Task HandleAsync(VoidCommand command, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    private static CommandStrategyContext BuildContext(
        ICommand command,
        ICommandStrategy strategy,
        ICommandHandler handler,
        Type? resultType,
        IServiceProvider? serviceProvider = null)
    {
        var sp = serviceProvider ?? Substitute.For<IServiceProvider>();
        var commandContext = new CommandContext(command, strategy, resultType, sp, default);
        return new CommandStrategyContext(commandContext, handler, sp);
    }

    [Fact]
    public async Task DefaultStrategy_ExecuteAsync_WithResult_ReturnsHandlerResult()
    {
        var handler = new SumCommandHandler();
        var strategy = CommandStrategy.Default;
        var context = BuildContext(new SumCommand(3, 4), strategy, handler, typeof(int));

        var result = await strategy.ExecuteAsync<int>(context);

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task DefaultStrategy_ExecuteAsync_Void_InvokesHandler()
    {
        var handler = new VoidCommandHandler();
        var strategy = CommandStrategy.Default;
        var context = BuildContext(new VoidCommand(), strategy, handler, null);

        await strategy.ExecuteAsync(context);

        Assert.True(handler.Called);
    }

    [Fact]
    public async Task BackgroundStrategy_ExecuteAsync_Void_WritesCommandContextToChannel()
    {
        var innerChannel = Channel.CreateUnbounded<CommandContext>();
        var commandsChannel = Substitute.For<ICommandsChannel>();
        commandsChannel.Writer.Returns(innerChannel.Writer);
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(ICommandsChannel)).Returns(commandsChannel);

        var handler = Substitute.For<ICommandHandler>();
        var strategy = CommandStrategy.Background;
        var context = BuildContext(new VoidCommand(), strategy, handler, null, sp);

        await strategy.ExecuteAsync(context);

        Assert.True(innerChannel.Reader.TryRead(out var written));
        Assert.Same(context.CommandContext, written);
    }

    [Fact]
    public async Task BackgroundStrategy_ExecuteAsync_WithResult_WritesCommandContextToChannel_AndReturnsDefault()
    {
        var innerChannel = Channel.CreateUnbounded<CommandContext>();
        var commandsChannel = Substitute.For<ICommandsChannel>();
        commandsChannel.Writer.Returns(innerChannel.Writer);
        var sp = Substitute.For<IServiceProvider>();
        sp.GetService(typeof(ICommandsChannel)).Returns(commandsChannel);

        var handler = Substitute.For<ICommandHandler>();
        var strategy = CommandStrategy.Background;
        var context = BuildContext(new SumCommand(1, 2), strategy, handler, typeof(int), sp);

        var result = await strategy.ExecuteAsync<int>(context);

        Assert.True(innerChannel.Reader.TryRead(out var written));
        Assert.Same(context.CommandContext, written);
        Assert.Equal(default, result);
    }
}
