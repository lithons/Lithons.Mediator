using BenchmarkDotNet.Attributes;
using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Lithons.Mediator.Extensions;
using Lithons.Mediator.Middleware.Command;
using Lithons.Mediator.Middleware.Notification;
using Lithons.Mediator.Middleware.Request;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lithons.Mediator.Benchmarks;

[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private record EchoRequest(string Value) : IRequest<string>;

    private class EchoRequestHandler : IRequestHandler<EchoRequest, string>
    {
        public Task<string> Handle(EchoRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    private record AddCommand(int A, int B) : ICommand<int>;

    private class AddCommandHandler : ICommandHandler<AddCommand, int>
    {
        public Task<int> Handle(AddCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.A + command.B);
    }

    private record PingCommand : ICommand;

    private class PingCommandHandler : ICommandHandler<PingCommand>
    {
        public Task Handle(PingCommand command, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private record OrderPlacedNotification(int OrderId) : INotification;

    private class OrderHandler1 : INotificationHandler<OrderPlacedNotification>
    {
        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private class OrderHandler2 : INotificationHandler<OrderPlacedNotification>
    {
        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private class OrderHandler3 : INotificationHandler<OrderPlacedNotification>
    {
        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private class OrderHandler4 : INotificationHandler<OrderPlacedNotification>
    {
        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private class OrderHandler5 : INotificationHandler<OrderPlacedNotification>
    {
        public Task Handle(OrderPlacedNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private static readonly EchoRequest _echoRequest = new("hello");
    private static readonly AddCommand _addCommand = new(3, 4);
    private static readonly PingCommand _pingCommand = new();
    private static readonly OrderPlacedNotification _notification = new(42);

    private IServiceScope _scope = null!;
    private IMediator _mediator = null!;

    private IServiceScope _middlewareScope = null!;
    private IMediator _middlewareMediator = null!;

    private IServiceScope _manyHandlersScope = null!;
    private IMediator _manyHandlersMediator = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Plain mediator (no middleware, 2 notification handlers)
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(cfg =>
        {
            cfg.AddRequestHandler<EchoRequestHandler>();
            cfg.AddCommandHandler<AddCommandHandler>();
            cfg.AddCommandHandler<PingCommandHandler>();
            cfg.AddNotificationHandler<OrderHandler1>();
            cfg.AddNotificationHandler<OrderHandler2>();
        });

        var provider = services.BuildServiceProvider();
        _scope = provider.CreateScope();
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();

        // Mediator with a single no-op middleware on each pipeline
        var middlewareServices = new ServiceCollection();
        middlewareServices.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        middlewareServices.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        middlewareServices.AddMediator();
        middlewareServices.AddRequestHandler<EchoRequestHandler>();
        middlewareServices.AddCommandHandler<AddCommandHandler>();
        middlewareServices.AddCommandHandler<PingCommandHandler>();
        middlewareServices.AddNotificationHandler<OrderHandler1>();
        middlewareServices.AddNotificationHandler<OrderHandler2>();

        var middlewareProvider = middlewareServices.BuildServiceProvider();

        var requestPipeline = middlewareProvider.GetRequiredService<IRequestPipeline>();
        requestPipeline.Setup(b =>
        {
            b.Use(next => async ctx => { await next(ctx); });
            b.UseRequestHandlers();
        });

        var commandPipeline = middlewareProvider.GetRequiredService<ICommandPipeline>();
        commandPipeline.Setup(b =>
        {
            b.Use(next => async ctx => { await next(ctx); });
            b.UseCommandHandlers();
        });

        var notificationPipeline = middlewareProvider.GetRequiredService<INotificationPipeline>();
        notificationPipeline.Setup(b =>
        {
            b.Use(next => async ctx => { await next(ctx); });
            b.UseNotificationHandlers();
        });

        _middlewareScope = middlewareProvider.CreateScope();
        _middlewareMediator = _middlewareScope.ServiceProvider.GetRequiredService<IMediator>();

        // Mediator with 5 notification handlers for scaling benchmarks
        var manyServices = new ServiceCollection();
        manyServices.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        manyServices.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        manyServices.AddMediator();
        manyServices.AddNotificationHandler<OrderHandler1>();
        manyServices.AddNotificationHandler<OrderHandler2>();
        manyServices.AddNotificationHandler<OrderHandler3>();
        manyServices.AddNotificationHandler<OrderHandler4>();
        manyServices.AddNotificationHandler<OrderHandler5>();

        var manyProvider = manyServices.BuildServiceProvider();
        _manyHandlersScope = manyProvider.CreateScope();
        _manyHandlersMediator = _manyHandlersScope.ServiceProvider.GetRequiredService<IMediator>();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _scope.Dispose();
        _middlewareScope.Dispose();
        _manyHandlersScope.Dispose();
    }

    // --- Baseline (no middleware) ---

    [Benchmark]
    public Task<string> SendRequest()
        => _mediator.SendAsync(_echoRequest);

    [Benchmark]
    public Task<int> SendCommand_WithResult()
        => _mediator.SendAsync(_addCommand);

    [Benchmark]
    public Task SendCommand_Void()
        => _mediator.SendAsync(_pingCommand);

    [Benchmark]
    public Task SendNotification_Sequential()
        => _mediator.SendAsync(_notification);

    [Benchmark]
    public Task SendNotification_Parallel()
        => _mediator.SendAsync(_notification, NotificationStrategy.Parallel);

    // --- Explicit command strategy ---

    [Benchmark]
    public Task<int> SendCommand_WithResult_ExplicitStrategy()
        => _mediator.SendAsync(_addCommand, CommandStrategy.Default);

    [Benchmark]
    public Task SendCommand_Void_ExplicitStrategy()
        => _mediator.SendAsync(_pingCommand, CommandStrategy.Default);

    // --- With middleware pipeline ---

    [Benchmark]
    public Task<string> SendRequest_WithMiddleware()
        => _middlewareMediator.SendAsync(_echoRequest);

    [Benchmark]
    public Task<int> SendCommand_WithResult_WithMiddleware()
        => _middlewareMediator.SendAsync(_addCommand);

    [Benchmark]
    public Task SendCommand_Void_WithMiddleware()
        => _middlewareMediator.SendAsync(_pingCommand);

    [Benchmark]
    public Task SendNotification_Sequential_WithMiddleware()
        => _middlewareMediator.SendAsync(_notification);

    // --- Notification handler scaling (5 handlers) ---

    [Benchmark]
    public Task SendNotification_Sequential_FiveHandlers()
        => _manyHandlersMediator.SendAsync(_notification);

    [Benchmark]
    public Task SendNotification_Parallel_FiveHandlers()
        => _manyHandlersMediator.SendAsync(_notification, NotificationStrategy.Parallel);
}