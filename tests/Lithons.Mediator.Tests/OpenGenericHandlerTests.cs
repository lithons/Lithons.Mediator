using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lithons.Mediator.Tests;

public class OpenGenericHandlerTests
{
    private record GetById(int Id) : IRequest<string>;
    private record GetByName(string Name) : IRequest<string>;
    private record DoSomething(string Value) : ICommand;
    private record AddNumbers(int A, int B) : ICommand<int>;
    private record SomethingHappened(string Info) : INotification;

    private class CatchAllRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
            => Task.FromResult(default(TResponse)!);
    }

    private class CatchAllCommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public Task Handle(TCommand command, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private class CatchAllCommandWithResultHandler<TCommand, TResult> : ICommandHandler<TCommand, TResult>
        where TCommand : ICommand
    {
        public Task<TResult> Handle(TCommand command, CancellationToken cancellationToken)
            => Task.FromResult(default(TResult)!);
    }

    private class CatchAllNotificationHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : INotification
    {
        public Task Handle(TNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private static IServiceScope CreateScope(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator();
        configure(services);
        return services.BuildServiceProvider().CreateScope();
    }

    #region AddRequestHandler (open generic)

    [Fact]
    public async Task AddRequestHandler_OpenGeneric_ResolvesForAnyRequestType()
    {
        using var scope = CreateScope(s =>
            s.AddRequestHandler(typeof(CatchAllRequestHandler<,>)));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new GetById(1), TestContext.Current.CancellationToken);
        await mediator.SendAsync(new GetByName("test"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public void AddRequestHandler_NonGenericType_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateScope(s => s.AddRequestHandler(typeof(string))));
    }

    [Fact]
    public void AddRequestHandler_OpenGeneric_WrongInterface_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateScope(s => s.AddRequestHandler(typeof(CatchAllNotificationHandler<>))));
    }

    #endregion

    #region AddCommandHandler (open generic)

    [Fact]
    public async Task AddCommandHandler_OpenGeneric_Void_ResolvesForAnyCommandType()
    {
        using var scope = CreateScope(s =>
            s.AddCommandHandler(typeof(CatchAllCommandHandler<>)));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new DoSomething("x"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task AddCommandHandler_OpenGeneric_WithResult_ResolvesForAnyCommandType()
    {
        using var scope = CreateScope(s =>
            s.AddCommandHandler(typeof(CatchAllCommandWithResultHandler<,>)));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new AddNumbers(1, 2), TestContext.Current.CancellationToken);
    }

    [Fact]
    public void AddCommandHandler_OpenGeneric_WrongInterface_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateScope(s => s.AddCommandHandler(typeof(CatchAllRequestHandler<,>))));
    }

    #endregion

    #region AddNotificationHandler (open generic)

    [Fact]
    public async Task AddNotificationHandler_OpenGeneric_ResolvesForAnyNotificationType()
    {
        using var scope = CreateScope(s =>
            s.AddNotificationHandler(typeof(CatchAllNotificationHandler<>)));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new SomethingHappened("info"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public void AddNotificationHandler_OpenGeneric_WrongInterface_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateScope(s => s.AddNotificationHandler(typeof(CatchAllRequestHandler<,>))));
    }

    #endregion

    #region Assembly Scanning

    [Fact]
    public async Task AddHandlersFromAssembly_IncludesOpenGenericHandlers()
    {
        using var scope = CreateScope(s =>
            s.AddHandlersFromAssembly(
                typeof(OpenGenericHandlerTests).Assembly,
                t => t == typeof(CatchAllRequestHandler<,>)));
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.SendAsync(new GetById(1), TestContext.Current.CancellationToken);
        await mediator.SendAsync(new GetByName("test"), TestContext.Current.CancellationToken);
    }

    #endregion
}
