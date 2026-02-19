using Lithons.Mediator.Contracts;
using Lithons.Mediator.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
        public Task<string> HandleAsync(EchoRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    private class EchoRequestHandlerDuplicate : IRequestHandler<EchoRequest, string>
    {
        public Task<string> HandleAsync(EchoRequest request, CancellationToken cancellationToken)
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
        public Task<int> HandleAsync(AddCommand command, CancellationToken cancellationToken)
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

        public Task HandleAsync(PingCommand command, CancellationToken cancellationToken)
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

        public Task HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken)
        {
            Received.Add(notification.OrderId);
            return Task.CompletedTask;
        }
    }

    private class OrderHandler2 : INotificationHandler<OrderPlacedNotification>
    {
        public bool Called { get; private set; }

        public Task HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken)
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
}
