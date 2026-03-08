using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Lithons.Mediator.Tests.Services;

public class CommandsBackgroundServiceTests
{
    #region Test types

    private record PingCommand : ICommand;

    private class PingCommandHandler : ICommandHandler<PingCommand>
    {
        public bool Called { get; private set; }

        public Task Handle(PingCommand command, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    private record AddCommand(int A, int B) : ICommand<int>;

    private class AddCommandHandler : ICommandHandler<AddCommand, int>
    {
        public int? Result { get; private set; }

        public Task<int> Handle(AddCommand command, CancellationToken cancellationToken)
        {
            Result = command.A + command.B;
            return Task.FromResult(Result.Value);
        }
    }

    private record FailingCommand : ICommand;

    private class FailingCommandHandler : ICommandHandler<FailingCommand>
    {
        public Task Handle(FailingCommand command, CancellationToken cancellationToken)
            => throw new InvalidOperationException("handler error");
    }

    #endregion

    private static (DefaultCommandsChannel channel, CommandsBackgroundService service, IServiceProvider sp)
        CreateService(Action<IServiceCollection> configure)
    {
        var channel = new DefaultCommandsChannel(Channel.CreateUnbounded<CommandContext>(
            new UnboundedChannelOptions { SingleReader = true }));

        var services = new ServiceCollection();
        configure(services);
        var sp = services.BuildServiceProvider();

        var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
        var logger = Substitute.For<ILogger<CommandsBackgroundService>>();
        var service = new CommandsBackgroundService(channel, scopeFactory, logger);

        return (channel, service, sp);
    }

    private static CommandContext CreateCommandContext(
        ICommand command,
        Type? resultType,
        IServiceProvider serviceProvider)
    {
        return new CommandContext(command, CommandStrategy.Background, resultType, serviceProvider, default);
    }

    [Fact]
    public async Task ExecuteAsync_VoidCommand_InvokesHandler()
    {
        var handler = new PingCommandHandler();
        var (channel, service, sp) = CreateService(s =>
            s.AddSingleton<ICommandHandler<PingCommand>>(handler));

        var ctx = CreateCommandContext(new PingCommand(), null, sp);
        await channel.Writer.WriteAsync(ctx);
        channel.Writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        Assert.True(handler.Called);
    }

    [Fact]
    public async Task ExecuteAsync_ResultCommand_InvokesHandler()
    {
        var handler = new AddCommandHandler();
        var (channel, service, sp) = CreateService(s =>
            s.AddSingleton<ICommandHandler<AddCommand, int>>(handler));

        var ctx = CreateCommandContext(new AddCommand(3, 4), typeof(int), sp);
        await channel.Writer.WriteAsync(ctx);
        channel.Writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        Assert.Equal(7, handler.Result);
    }

    [Fact]
    public async Task ExecuteAsync_HandlerThrows_ContinuesProcessing()
    {
        var pingHandler = new PingCommandHandler();
        var (channel, service, sp) = CreateService(s =>
        {
            s.AddSingleton<ICommandHandler<FailingCommand>>(new FailingCommandHandler());
            s.AddSingleton<ICommandHandler<PingCommand>>(pingHandler);
        });

        await channel.Writer.WriteAsync(CreateCommandContext(new FailingCommand(), null, sp));
        await channel.Writer.WriteAsync(CreateCommandContext(new PingCommand(), null, sp));
        channel.Writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        Assert.True(pingHandler.Called);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationStopsProcessing()
    {
        var handler = new PingCommandHandler();
        var (channel, service, _) = CreateService(s =>
            s.AddSingleton<ICommandHandler<PingCommand>>(handler));

        using var cts = new CancellationTokenSource();
        await service.StartAsync(cts.Token);

        // Cancel before writing any commands
        await cts.CancelAsync();

        // Allow the service to observe cancellation
        try
        {
            await service.ExecuteTask!;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        Assert.False(handler.Called);
    }

    [Fact]
    public async Task ExecuteAsync_MultipleCommands_ProcessedInOrder()
    {
        var order = new List<int>();
        var handler1 = new OrderTrackingHandler(1, order);
        var handler2 = new OrderTrackingHandler(2, order);
        var handler3 = new OrderTrackingHandler(3, order);

        var (channel, service, sp) = CreateService(s =>
            s.AddSingleton<ICommandHandler<OrderedCommand>>(new OrderedCommandHandler(order)));

        await channel.Writer.WriteAsync(CreateCommandContext(new OrderedCommand(1), null, sp));
        await channel.Writer.WriteAsync(CreateCommandContext(new OrderedCommand(2), null, sp));
        await channel.Writer.WriteAsync(CreateCommandContext(new OrderedCommand(3), null, sp));
        channel.Writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        Assert.Equal([1, 2, 3], order);
    }

    private record OrderedCommand(int Id) : ICommand;

    private class OrderedCommandHandler(List<int> order) : ICommandHandler<OrderedCommand>
    {
        public Task Handle(OrderedCommand command, CancellationToken cancellationToken)
        {
            order.Add(command.Id);
            return Task.CompletedTask;
        }
    }

    private class OrderTrackingHandler(int id, List<int> order) : ICommandHandler<PingCommand>
    {
        public Task Handle(PingCommand command, CancellationToken cancellationToken)
        {
            order.Add(id);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesNewScopePerCommand()
    {
        var scopeIds = new List<Guid>();
        var (channel, service, sp) = CreateService(s =>
            s.AddScoped<ICommandHandler<PingCommand>>(_ =>
            {
                scopeIds.Add(Guid.NewGuid());
                return new PingCommandHandler();
            }));

        await channel.Writer.WriteAsync(CreateCommandContext(new PingCommand(), null, sp));
        await channel.Writer.WriteAsync(CreateCommandContext(new PingCommand(), null, sp));
        channel.Writer.Complete();

        await service.StartAsync(TestContext.Current.CancellationToken);
        await service.ExecuteTask!;

        Assert.Equal(2, scopeIds.Count);
        Assert.NotEqual(scopeIds[0], scopeIds[1]);
    }
}
