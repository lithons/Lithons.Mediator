using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Exceptions;
using Lithons.Mediator.Extensions;
using Lithons.Mediator.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lithons.Mediator.Tests;

public class ExceptionHandlerTests
{
    private static IServiceScope CreateScope(
        Action<MediatorConfiguration>? configureMediatorAction = null,
        Action<IServiceCollection>? configureServices = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        configureServices?.Invoke(services);
        services.AddMediator(configureMediatorAction);
        return services.BuildServiceProvider().CreateScope();
    }

    #region Test messages & handlers

    private record FailingRequest(string Value) : IRequest<string>;

    private class FailingRequestHandler : IRequestHandler<FailingRequest, string>
    {
        public Task<string> Handle(FailingRequest request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Request failed");
    }

    private record FailingCommand(string Value) : ICommand;

    private class FailingCommandHandler : ICommandHandler<FailingCommand>
    {
        public Task Handle(FailingCommand command, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Command failed");
    }

    private record FailingResultCommand(int A) : ICommand<int>;

    private class FailingResultCommandHandler : ICommandHandler<FailingResultCommand, int>
    {
        public Task<int> Handle(FailingResultCommand command, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Result command failed");
    }

    private record FailingNotification(int Id) : INotification;

    private class FailingNotificationHandler : INotificationHandler<FailingNotification>
    {
        public Task Handle(FailingNotification notification, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Notification failed");
    }

    private record SuccessRequest(string Value) : IRequest<string>;

    private class SuccessRequestHandler : IRequestHandler<SuccessRequest, string>
    {
        public Task<string> Handle(SuccessRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    #endregion

    #region Global exception handler

    private class TrackingGlobalExceptionHandler : IExceptionHandler
    {
        public Exception? LastException { get; private set; }
        public object? LastMessage { get; private set; }
        public bool ShouldHandle { get; set; } = true;

        public ValueTask<bool> Handle(Exception exception, object message, CancellationToken cancellationToken)
        {
            LastException = exception;
            LastMessage = message;
            return ValueTask.FromResult(ShouldHandle);
        }
    }

    [Fact]
    public async Task Request_GlobalExceptionHandler_HandlesException()
    {
        var exHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<FailingRequestHandler>(),
            s => s.AddSingleton<IExceptionHandler>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken);

        Assert.NotNull(exHandler.LastException);
        Assert.IsType<InvalidOperationException>(exHandler.LastException);
        Assert.IsType<FailingRequest>(exHandler.LastMessage);
    }

    [Fact]
    public async Task Request_GlobalExceptionHandler_NotHandled_Rethrows()
    {
        var exHandler = new TrackingGlobalExceptionHandler { ShouldHandle = false };
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<FailingRequestHandler>(),
            s => s.AddSingleton<IExceptionHandler>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken));

        Assert.NotNull(exHandler.LastException);
    }

    [Fact]
    public async Task VoidCommand_GlobalExceptionHandler_HandlesException()
    {
        var exHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddCommandHandler<FailingCommandHandler>(),
            s => s.AddSingleton<IExceptionHandler>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingCommand("test"), TestContext.Current.CancellationToken);

        Assert.NotNull(exHandler.LastException);
        Assert.IsType<FailingCommand>(exHandler.LastMessage);
    }

    [Fact]
    public async Task ResultCommand_GlobalExceptionHandler_HandlesException()
    {
        var exHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddCommandHandler<FailingResultCommandHandler>(),
            s => s.AddSingleton<IExceptionHandler>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingResultCommand(1), TestContext.Current.CancellationToken);

        Assert.NotNull(exHandler.LastException);
        Assert.IsType<FailingResultCommand>(exHandler.LastMessage);
    }

    [Fact]
    public async Task Notification_GlobalExceptionHandler_HandlesException()
    {
        var exHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            configureServices: s =>
            {
                s.AddSingleton<INotificationHandler<FailingNotification>>(new FailingNotificationHandler());
                s.AddSingleton<IExceptionHandler>(exHandler);
            });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingNotification(42), TestContext.Current.CancellationToken);

        Assert.NotNull(exHandler.LastException);
        Assert.IsType<FailingNotification>(exHandler.LastMessage);
    }

    #endregion

    #region Typed exception handler

    private class TypedRequestExceptionHandler : IExceptionHandler<FailingRequest>
    {
        public Exception? LastException { get; private set; }
        public FailingRequest? LastMessage { get; private set; }
        public bool ShouldHandle { get; set; } = true;

        public ValueTask<bool> Handle(Exception exception, FailingRequest message, CancellationToken cancellationToken)
        {
            LastException = exception;
            LastMessage = message;
            return ValueTask.FromResult(ShouldHandle);
        }
    }

    private class TypedCommandExceptionHandler : IExceptionHandler<FailingCommand>
    {
        public bool Called { get; private set; }
        public bool ShouldHandle { get; set; } = true;

        public ValueTask<bool> Handle(Exception exception, FailingCommand message, CancellationToken cancellationToken)
        {
            Called = true;
            return ValueTask.FromResult(ShouldHandle);
        }
    }

    private class TypedNotificationExceptionHandler : IExceptionHandler<FailingNotification>
    {
        public bool Called { get; private set; }
        public bool ShouldHandle { get; set; } = true;

        public ValueTask<bool> Handle(Exception exception, FailingNotification message, CancellationToken cancellationToken)
        {
            Called = true;
            return ValueTask.FromResult(ShouldHandle);
        }
    }

    [Fact]
    public async Task Request_TypedExceptionHandler_HandlesException()
    {
        var exHandler = new TypedRequestExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<FailingRequestHandler>(),
            s => s.AddSingleton<IExceptionHandler<FailingRequest>>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken);

        Assert.NotNull(exHandler.LastException);
        Assert.Equal("test", exHandler.LastMessage?.Value);
    }

    [Fact]
    public async Task Request_TypedExceptionHandler_NotHandled_Rethrows()
    {
        var exHandler = new TypedRequestExceptionHandler { ShouldHandle = false };
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<FailingRequestHandler>(),
            s => s.AddSingleton<IExceptionHandler<FailingRequest>>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken));

        Assert.NotNull(exHandler.LastException);
    }

    [Fact]
    public async Task VoidCommand_TypedExceptionHandler_HandlesException()
    {
        var exHandler = new TypedCommandExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddCommandHandler<FailingCommandHandler>(),
            s => s.AddSingleton<IExceptionHandler<FailingCommand>>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingCommand("test"), TestContext.Current.CancellationToken);

        Assert.True(exHandler.Called);
    }

    [Fact]
    public async Task Notification_TypedExceptionHandler_HandlesException()
    {
        var exHandler = new TypedNotificationExceptionHandler();
        using var scope = CreateScope(
            configureServices: s =>
            {
                s.AddSingleton<INotificationHandler<FailingNotification>>(new FailingNotificationHandler());
                s.AddSingleton<IExceptionHandler<FailingNotification>>(exHandler);
            });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingNotification(1), TestContext.Current.CancellationToken);

        Assert.True(exHandler.Called);
    }

    #endregion

    #region Typed takes precedence over global

    [Fact]
    public async Task Request_TypedHandler_TakesPrecedence_OverGlobal()
    {
        var typedHandler = new TypedRequestExceptionHandler();
        var globalHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<FailingRequestHandler>(),
            s =>
            {
                s.AddSingleton<IExceptionHandler<FailingRequest>>(typedHandler);
                s.AddSingleton<IExceptionHandler>(globalHandler);
            });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken);

        Assert.NotNull(typedHandler.LastException);
        Assert.Null(globalHandler.LastException);
    }

    [Fact]
    public async Task Request_TypedNotHandled_FallsBackToGlobal()
    {
        var typedHandler = new TypedRequestExceptionHandler { ShouldHandle = false };
        var globalHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<FailingRequestHandler>(),
            s =>
            {
                s.AddSingleton<IExceptionHandler<FailingRequest>>(typedHandler);
                s.AddSingleton<IExceptionHandler>(globalHandler);
            });
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken);

        Assert.NotNull(typedHandler.LastException);
        Assert.NotNull(globalHandler.LastException);
    }

    #endregion

    #region No handler registered — exception propagates

    [Fact]
    public async Task Request_NoExceptionHandler_ExceptionPropagates()
    {
        using var scope = CreateScope(cfg => cfg.AddRequestHandler<FailingRequestHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task VoidCommand_NoExceptionHandler_ExceptionPropagates()
    {
        using var scope = CreateScope(cfg => cfg.AddCommandHandler<FailingCommandHandler>());
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(new FailingCommand("test"), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Notification_NoExceptionHandler_ExceptionPropagates()
    {
        using var scope = CreateScope(
            configureServices: s =>
                s.AddSingleton<INotificationHandler<FailingNotification>>(new FailingNotificationHandler()));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(new FailingNotification(1), TestContext.Current.CancellationToken));
    }

    #endregion

    #region OperationCanceledException is never caught

    [Fact]
    public async Task Request_OperationCanceled_NotCaughtByExceptionHandler()
    {
        var exHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<CancellingRequestHandler>(),
            s => s.AddSingleton<IExceptionHandler>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => mediator.SendAsync(new CancellingRequest(), TestContext.Current.CancellationToken));

        Assert.Null(exHandler.LastException);
    }

    private record CancellingRequest : IRequest<string>;

    private class CancellingRequestHandler : IRequestHandler<CancellingRequest, string>
    {
        public Task<string> Handle(CancellingRequest request, CancellationToken cancellationToken)
            => throw new OperationCanceledException();
    }

    #endregion

    #region Happy path — no overhead when no exception

    [Fact]
    public async Task Request_NoException_ExceptionHandlerNotInvoked()
    {
        var exHandler = new TrackingGlobalExceptionHandler();
        using var scope = CreateScope(
            cfg => cfg.AddRequestHandler<SuccessRequestHandler>(),
            s => s.AddSingleton<IExceptionHandler>(exHandler));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new SuccessRequest("ok"), TestContext.Current.CancellationToken);

        Assert.Equal("ok", result);
        Assert.Null(exHandler.LastException);
    }

    #endregion

    #region Registration via MediatorConfiguration

    [Fact]
    public async Task MediatorConfiguration_AddExceptionHandler_Global()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(cfg =>
        {
            cfg.AddExceptionHandler<SwallowAllExceptionHandler>();
            cfg.AddRequestHandler<FailingRequestHandler>();
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task MediatorConfiguration_AddExceptionHandler_Typed()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(cfg =>
        {
            cfg.AddExceptionHandler<FailingRequest, SwallowFailingRequestExceptionHandler>();
            cfg.AddRequestHandler<FailingRequestHandler>();
        });
        using var scope = services.BuildServiceProvider().CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new FailingRequest("test"), TestContext.Current.CancellationToken);
    }

    private class SwallowAllExceptionHandler : IExceptionHandler
    {
        public ValueTask<bool> Handle(Exception exception, object message, CancellationToken cancellationToken)
            => ValueTask.FromResult(true);
    }

    private class SwallowFailingRequestExceptionHandler : IExceptionHandler<FailingRequest>
    {
        public ValueTask<bool> Handle(Exception exception, FailingRequest message, CancellationToken cancellationToken)
            => ValueTask.FromResult(true);
    }

    #endregion
}
