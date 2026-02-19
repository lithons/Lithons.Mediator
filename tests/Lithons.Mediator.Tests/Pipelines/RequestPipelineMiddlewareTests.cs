using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Lithons.Mediator.Middleware.Command;
using Lithons.Mediator.Middleware.Notification;
using Lithons.Mediator.Middleware.Request;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lithons.Mediator.Tests.Pipelines;

public class RequestPipelineMiddlewareTests
{
    private record EchoRequest(string Value) : IRequest<string>;
    private class EchoRequestHandler : IRequestHandler<EchoRequest, string>
    {
        public Task<string> HandleAsync(EchoRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    private static ServiceProvider BuildServiceProvider(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task RequestPipeline_CustomMiddleware_ExecutesBeforeHandler()
    {
        var sp = BuildServiceProvider(s => s.AddRequestHandler<EchoRequestHandler>());
        var pipeline = sp.GetRequiredService<IRequestPipeline>();
        var middlewareCalled = false;

        pipeline.Setup(b =>
        {
            b.Use(next => async ctx =>
            {
                middlewareCalled = true;
                await next(ctx);
            });
            b.UseRequestHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new EchoRequest("test"), TestContext.Current.CancellationToken);

        Assert.True(middlewareCalled);
    }

    [Fact]
    public async Task RequestPipeline_MultipleMiddlewares_ExecuteInRegistrationOrder()
    {
        var sp = BuildServiceProvider(s => s.AddRequestHandler<EchoRequestHandler>());
        var pipeline = sp.GetRequiredService<IRequestPipeline>();
        var order = new List<int>();

        pipeline.Setup(b =>
        {
            b.Use(next => async ctx => { order.Add(1); await next(ctx); });
            b.Use(next => async ctx => { order.Add(2); await next(ctx); });
            b.UseRequestHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new EchoRequest("test"), TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], order);
    }

    [Fact]
    public async Task RequestPipeline_MiddlewareCanShortCircuit_HandlerNotCalled()
    {
        var sp = BuildServiceProvider(s => s.AddRequestHandler<EchoRequestHandler>());
        var pipeline = sp.GetRequiredService<IRequestPipeline>();
        var handlerCalled = false;

        pipeline.Setup(b =>
        {
            b.Use(_ => ctx =>
            {
                ctx.Response = "short-circuited";
                return ValueTask.CompletedTask;
            });
            b.Use(next => async ctx =>
            {
                handlerCalled = true;
                await next(ctx);
            });
            b.UseRequestHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var result = await mediator.SendAsync(new EchoRequest("ignored"), TestContext.Current.CancellationToken);

        Assert.Equal("short-circuited", result);
        Assert.False(handlerCalled);
    }

    private record PingCommand : ICommand;
    private class PingCommandHandler : ICommandHandler<PingCommand>
    {
        public Task HandleAsync(PingCommand command, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task CommandPipeline_CustomMiddleware_ExecutesBeforeHandler()
    {
        var sp = BuildServiceProvider(s => s.AddCommandHandler<PingCommandHandler>());
        var pipeline = sp.GetRequiredService<ICommandPipeline>();
        var middlewareCalled = false;

        pipeline.Setup(b =>
        {
            b.Use(next => async ctx =>
            {
                middlewareCalled = true;
                await next(ctx);
            });
            b.UseCommandHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new PingCommand(), TestContext.Current.CancellationToken);

        Assert.True(middlewareCalled);
    }

    [Fact]
    public async Task CommandPipeline_MultipleMiddlewares_ExecuteInRegistrationOrder()
    {
        var sp = BuildServiceProvider(s => s.AddCommandHandler<PingCommandHandler>());
        var pipeline = sp.GetRequiredService<ICommandPipeline>();
        var order = new List<int>();

        pipeline.Setup(b =>
        {
            b.Use(next => async ctx => { order.Add(1); await next(ctx); });
            b.Use(next => async ctx => { order.Add(2); await next(ctx); });
            b.UseCommandHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new PingCommand(), TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], order);
    }

    [Fact]
    public async Task CommandPipeline_MiddlewareCanShortCircuit_HandlerNotCalled()
    {
        var sp = BuildServiceProvider(s => s.AddCommandHandler<PingCommandHandler>());
        var pipeline = sp.GetRequiredService<ICommandPipeline>();
        var handlerCalled = false;

        pipeline.Setup(b =>
        {
            b.Use(_ => _ => ValueTask.CompletedTask);
            b.Use(next => async ctx =>
            {
                handlerCalled = true;
                await next(ctx);
            });
            b.UseCommandHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new PingCommand(), TestContext.Current.CancellationToken);

        Assert.False(handlerCalled);
    }

    private record TestNotification : INotification;
    private class TestNotificationHandler : INotificationHandler<TestNotification>
    {
        public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public async Task NotificationPipeline_CustomMiddleware_ExecutesBeforeHandler()
    {
        var sp = BuildServiceProvider(s => s.AddNotificationHandler<TestNotificationHandler>());
        var pipeline = sp.GetRequiredService<INotificationPipeline>();
        var middlewareCalled = false;

        pipeline.Setup(b =>
        {
            b.Use(next => async ctx =>
            {
                middlewareCalled = true;
                await next(ctx);
            });
            b.UseNotificationHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new TestNotification(), TestContext.Current.CancellationToken);

        Assert.True(middlewareCalled);
    }

    [Fact]
    public async Task NotificationPipeline_MultipleMiddlewares_ExecuteInRegistrationOrder()
    {
        var sp = BuildServiceProvider(s => s.AddNotificationHandler<TestNotificationHandler>());
        var pipeline = sp.GetRequiredService<INotificationPipeline>();
        var order = new List<int>();

        pipeline.Setup(b =>
        {
            b.Use(next => async ctx => { order.Add(1); await next(ctx); });
            b.Use(next => async ctx => { order.Add(2); await next(ctx); });
            b.UseNotificationHandlers();
        });

        using var scope = sp.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new TestNotification(), TestContext.Current.CancellationToken);

        Assert.Equal([1, 2], order);
    }
}
