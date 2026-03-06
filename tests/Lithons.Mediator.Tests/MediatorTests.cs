using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Exceptions;
using Lithons.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Channels;

namespace Lithons.Mediator.Tests;

public class MediatorTests
{
    private static IServiceScope CreateScope(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator();
        configure(services);
        return services.BuildServiceProvider().CreateScope();
    }

    #region Requests

    private record EchoRequest(string Value) : IRequest<string>;

    private class EchoRequestHandler : IRequestHandler<EchoRequest, string>
    {
        public Task<string> Handle(EchoRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    private class EchoRequestHandlerDuplicate : IRequestHandler<EchoRequest, string>
    {
        public Task<string> Handle(EchoRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    [Fact]
    public async Task SendAsync_Request_ReturnsHandlerResult()
    {
        using var scope = CreateScope(s => s.AddRequestHandler<EchoRequestHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new EchoRequest("hello"), TestContext.Current.CancellationToken);

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task SendAsync_Request_NoHandler_ThrowsInvalidOperationException()
    {
        using var scope = CreateScope(_ => { });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<HandlerNotFoundException>(
            () => mediator.SendAsync(new EchoRequest("test"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public void AddRequestHandler_MultipleHandlers_ThrowsDuplicateHandlerException()
    {
        Assert.Throws<DuplicateHandlerException>(() =>
        {
            CreateScope(s =>
            {
                s.AddRequestHandler<EchoRequestHandler>();
                s.AddRequestHandler<EchoRequestHandlerDuplicate>();
            });
        });
    }

    #endregion

    #region Commands

    private record AddCommand(int A, int B) : ICommand<int>;

    private class AddCommandHandler : ICommandHandler<AddCommand, int>
    {
        public Task<int> Handle(AddCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.A + command.B);
    }

    [Fact]
    public async Task SendAsync_Command_WithResult_ReturnsHandlerResult()
    {
        using var scope = CreateScope(s => s.AddCommandHandler<AddCommandHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new AddCommand(3, 4), TestContext.Current.CancellationToken);

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task SendAsync_Command_NoHandler_ThrowsInvalidOperationException()
    {
        using var scope = CreateScope(_ => { });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<HandlerNotFoundException>(
            () => mediator.SendAsync(new AddCommand(1, 2), TestContext.Current.CancellationToken));
    }

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

    [Fact]
    public async Task SendAsync_Command_Void_ExecutesHandler()
    {
        using var scope = CreateScope(s => s.AddCommandHandler<PingCommandHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var handler = scope.ServiceProvider.GetRequiredService<PingCommandHandler>();

        await mediator.SendAsync(new PingCommand(), TestContext.Current.CancellationToken);

        Assert.True(handler.Called);
    }

    [Fact]
    public async Task SendAsync_Command_WithExplicitStrategy_UsesStrategy()
    {
        using var scope = CreateScope(s => s.AddCommandHandler<AddCommandHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new AddCommand(10, 5), CommandStrategy.Default, TestContext.Current.CancellationToken);

        Assert.Equal(15, result);
    }

    #endregion

    #region Notifications

    private record OrderPlacedNotification(int OrderId) : INotification;

    private class OrderHandler1 : INotificationHandler<OrderPlacedNotification>
    {
        public List<int> Received { get; } = [];

        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
        {
            Received.Add(notification.OrderId);
            return Task.CompletedTask;
        }
    }

    private class OrderHandler2 : INotificationHandler<OrderPlacedNotification>
    {
        public bool Called { get; private set; }

        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
        {
            Called = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SendAsync_Notification_SingleHandler_Invoked()
    {
        var handler = new OrderHandler1();
        using var scope = CreateScope(s => s.AddSingleton<INotificationHandler<OrderPlacedNotification>>(handler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new OrderPlacedNotification(42), TestContext.Current.CancellationToken);

        Assert.Equal([42], handler.Received);
    }

    [Fact]
    public async Task SendAsync_Notification_MultipleHandlers_AllInvoked()
    {
        var handler1 = new OrderHandler1();
        var handler2 = new OrderHandler2();
        using var scope = CreateScope(s =>
        {
            s.AddSingleton<INotificationHandler<OrderPlacedNotification>>(handler1);
            s.AddSingleton<INotificationHandler<OrderPlacedNotification>>(handler2);
        });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new OrderPlacedNotification(1), TestContext.Current.CancellationToken);

        Assert.Single(handler1.Received);
        Assert.True(handler2.Called);
    }

    [Fact]
    public async Task SendAsync_Notification_NoHandlers_DoesNotThrow()
    {
        using var scope = CreateScope(_ => { });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new OrderPlacedNotification(99), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task SendAsync_Notification_WithParallelStrategy_AllHandlersInvoked()
    {
        var handler1 = new OrderHandler1();
        var handler2 = new OrderHandler2();
        using var scope = CreateScope(s =>
        {
            s.AddSingleton<INotificationHandler<OrderPlacedNotification>>(handler1);
            s.AddSingleton<INotificationHandler<OrderPlacedNotification>>(handler2);
        });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new OrderPlacedNotification(7), NotificationStrategy.Parallel, TestContext.Current.CancellationToken);

        Assert.Single(handler1.Received);
        Assert.True(handler2.Called);
    }

    [Fact]
    public async Task SendAsync_Notification_HandlerReceivesCorrectData()
    {
        var handler = new OrderHandler1();
        using var scope = CreateScope(s => s.AddSingleton<INotificationHandler<OrderPlacedNotification>>(handler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new OrderPlacedNotification(10), TestContext.Current.CancellationToken);
        await mediator.SendAsync(new OrderPlacedNotification(20), TestContext.Current.CancellationToken);
        await mediator.SendAsync(new OrderPlacedNotification(30), TestContext.Current.CancellationToken);

        Assert.Equal([10, 20, 30], handler.Received);
    }

    #endregion

    #region CancellationToken

    private record TokenRequest : IRequest<CancellationToken>;

    private class TokenRequestHandler : IRequestHandler<TokenRequest, CancellationToken>
    {
        public Task<CancellationToken> Handle(TokenRequest request, CancellationToken cancellationToken)
            => Task.FromResult(cancellationToken);
    }

    [Fact]
    public async Task SendAsync_Request_PropagatesCancellationToken()
    {
        using var scope = CreateScope(s => s.AddRequestHandler<TokenRequestHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        using var cts = new CancellationTokenSource();

        var received = await mediator.SendAsync(new TokenRequest(), cts.Token);

        Assert.Equal(cts.Token, received);
    }

    private record TokenCommand : ICommand<CancellationToken>;

    private class TokenCommandHandler : ICommandHandler<TokenCommand, CancellationToken>
    {
        public Task<CancellationToken> Handle(TokenCommand command, CancellationToken cancellationToken)
            => Task.FromResult(cancellationToken);
    }

    [Fact]
    public async Task SendAsync_Command_PropagatesCancellationToken()
    {
        using var scope = CreateScope(s => s.AddCommandHandler<TokenCommandHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        using var cts = new CancellationTokenSource();

        var received = await mediator.SendAsync(new TokenCommand(), cts.Token);

        Assert.Equal(cts.Token, received);
    }

    #endregion

    #region Generics

    private record GenericRequest<T>(T Value) : IRequest<T>;

    private class GenericRequestHandler<T> : IRequestHandler<GenericRequest<T>, T>
    {
        public Task<T> Handle(GenericRequest<T> request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    [Fact]
    public async Task SendAsync_GenericRequest_String_ReturnsCorrectResult()
    {
        using var scope = CreateScope(s => s.AddRequestHandler<GenericRequestHandler<string>>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new GenericRequest<string>("hello"), TestContext.Current.CancellationToken);

        Assert.Equal("hello", result);
    }

    [Fact]
    public async Task SendAsync_GenericRequest_Int_ReturnsCorrectResult()
    {
        using var scope = CreateScope(s => s.AddRequestHandler<GenericRequestHandler<int>>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new GenericRequest<int>(42), TestContext.Current.CancellationToken);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task SendAsync_GenericRequest_MultipleTypeArgs_RoutedToCorrectHandler()
    {
        using var scope = CreateScope(s =>
        {
            s.AddRequestHandler<GenericRequestHandler<string>>();
            s.AddRequestHandler<GenericRequestHandler<int>>();
        });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var stringResult = await mediator.SendAsync(new GenericRequest<string>("test"), TestContext.Current.CancellationToken);
        var intResult = await mediator.SendAsync(new GenericRequest<int>(99), TestContext.Current.CancellationToken);

        Assert.Equal("test", stringResult);
        Assert.Equal(99, intResult);
    }

    private record GenericCommand<T>(T Value) : ICommand<T>;

    private class GenericCommandHandler<T> : ICommandHandler<GenericCommand<T>, T>
    {
        public Task<T> Handle(GenericCommand<T> command, CancellationToken cancellationToken)
            => Task.FromResult(command.Value);
    }

    [Fact]
    public async Task SendAsync_GenericCommand_String_ReturnsCorrectResult()
    {
        using var scope = CreateScope(s => s.AddCommandHandler<GenericCommandHandler<string>>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new GenericCommand<string>("world"), TestContext.Current.CancellationToken);

        Assert.Equal("world", result);
    }

    [Fact]
    public async Task SendAsync_GenericCommand_Int_ReturnsCorrectResult()
    {
        using var scope = CreateScope(s => s.AddCommandHandler<GenericCommandHandler<int>>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new GenericCommand<int>(7), TestContext.Current.CancellationToken);

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task SendAsync_GenericCommand_MultipleTypeArgs_RoutedToCorrectHandler()
    {
        using var scope = CreateScope(s =>
        {
            s.AddCommandHandler<GenericCommandHandler<string>>();
            s.AddCommandHandler<GenericCommandHandler<int>>();
        });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var stringResult = await mediator.SendAsync(new GenericCommand<string>("abc"), TestContext.Current.CancellationToken);
        var intResult = await mediator.SendAsync(new GenericCommand<int>(5), TestContext.Current.CancellationToken);

        Assert.Equal("abc", stringResult);
        Assert.Equal(5, intResult);
    }

    private record GenericNotification<T>(T Payload) : INotification;

    private class GenericNotificationHandler<T> : INotificationHandler<GenericNotification<T>>
    {
        public List<T> Received { get; } = [];

        public Task Handle(GenericNotification<T> notification, CancellationToken cancellationToken)
        {
            Received.Add(notification.Payload);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SendAsync_GenericNotification_String_InvokesHandler()
    {
        var handler = new GenericNotificationHandler<string>();
        using var scope = CreateScope(s => s.AddSingleton<INotificationHandler<GenericNotification<string>>>(handler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new GenericNotification<string>("event"), TestContext.Current.CancellationToken);

        Assert.Equal(["event"], handler.Received);
    }

    [Fact]
    public async Task SendAsync_GenericNotification_MultipleTypeArgs_RoutedToCorrectHandler()
    {
        var stringHandler = new GenericNotificationHandler<string>();
        var intHandler = new GenericNotificationHandler<int>();
        using var scope = CreateScope(s =>
        {
            s.AddSingleton<INotificationHandler<GenericNotification<string>>>(stringHandler);
            s.AddSingleton<INotificationHandler<GenericNotification<int>>>(intHandler);
        });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new GenericNotification<string>("hello"), TestContext.Current.CancellationToken);
        await mediator.SendAsync(new GenericNotification<int>(42), TestContext.Current.CancellationToken);

        Assert.Equal(["hello"], stringHandler.Received);
        Assert.Equal([42], intHandler.Received);
    }

    #endregion

    #region BackgroundStrategy

    private class InMemoryCommandsChannel : ICommandsChannel
    {
        private readonly Channel<CommandContext> _channel = Channel.CreateUnbounded<CommandContext>();
        public ChannelWriter<CommandContext> Writer => _channel.Writer;
        public ChannelReader<CommandContext> Reader => _channel.Reader;
    }

    private record BackgroundCommand(int Value) : ICommand<int>;

    private class BackgroundCommandHandler : ICommandHandler<BackgroundCommand, int>
    {
        public Task<int> Handle(BackgroundCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.Value);
    }

    [Fact]
    public async Task SendAsync_Command_WithBackgroundStrategy_WritesCommandToChannel()
    {
        var commandsChannel = new InMemoryCommandsChannel();
        using var scope = CreateScope(s =>
        {
            s.AddCommandHandler<BackgroundCommandHandler>();
            s.AddSingleton<ICommandsChannel>(commandsChannel);
        });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new BackgroundCommand(42), CommandStrategy.Background, TestContext.Current.CancellationToken);

        Assert.True(commandsChannel.Reader.TryRead(out var written));
        Assert.IsType<BackgroundCommand>(written.Command);
        Assert.Equal(42, ((BackgroundCommand)written.Command).Value);
    }

    #endregion
}
