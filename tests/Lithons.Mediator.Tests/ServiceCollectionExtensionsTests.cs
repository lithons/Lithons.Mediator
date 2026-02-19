using Lithons.Mediator.Contracts;
using Lithons.Mediator.Middleware.Command.Contracts;
using Lithons.Mediator.Middleware.Notification.Contracts;
using Lithons.Mediator.Middleware.Request.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lithons.Mediator.Tests;

public class ServiceCollectionExtensionsTests
{
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
    public void AddMediator_RegistersIMediator()
    {
        var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var mediator = scope.ServiceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddMediator_RegistersIRequestSender()
    {
        var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var sender = scope.ServiceProvider.GetService<IRequestSender>();

        Assert.NotNull(sender);
    }

    [Fact]
    public void AddMediator_RegistersICommandSender()
    {
        var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var sender = scope.ServiceProvider.GetService<ICommandSender>();

        Assert.NotNull(sender);
    }

    [Fact]
    public void AddMediator_RegistersINotificationSender()
    {
        var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var sender = scope.ServiceProvider.GetService<INotificationSender>();

        Assert.NotNull(sender);
    }

    [Fact]
    public void AddMediator_IRequestSender_IsSameInstanceAsIMediator()
    {
        var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var sender = scope.ServiceProvider.GetRequiredService<IRequestSender>();

        Assert.Same(mediator, sender);
    }

    [Fact]
    public void AddMediator_RegistersIRequestPipeline()
    {
        var sp = BuildServiceProvider();

        var pipeline = sp.GetService<IRequestPipeline>();

        Assert.NotNull(pipeline);
    }

    [Fact]
    public void AddMediator_RegistersICommandPipeline()
    {
        var sp = BuildServiceProvider();

        var pipeline = sp.GetService<ICommandPipeline>();

        Assert.NotNull(pipeline);
    }

    [Fact]
    public void AddMediator_RegistersINotificationPipeline()
    {
        var sp = BuildServiceProvider();

        var pipeline = sp.GetService<INotificationPipeline>();

        Assert.NotNull(pipeline);
    }

    private record SomeRequest(int Value) : IRequest<int>;
    private class SomeRequestHandler : IRequestHandler<SomeRequest, int>
    {
        public Task<int> HandleAsync(SomeRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    [Fact]
    public void AddRequestHandler_RegistersAsIRequestHandler()
    {
        var sp = BuildServiceProvider(s => s.AddRequestHandler<SomeRequestHandler>());

        var handlers = sp.GetServices<IRequestHandler>();

        Assert.Single(handlers);
        Assert.IsType<SomeRequestHandler>(handlers.First());
    }

    private record SomeCommand(int Value) : ICommand<int>;
    private class SomeCommandHandler : ICommandHandler<SomeCommand, int>
    {
        public Task<int> HandleAsync(SomeCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.Value);
    }

    [Fact]
    public void AddCommandHandler_RegistersAsICommandHandler()
    {
        var sp = BuildServiceProvider(s => s.AddCommandHandler<SomeCommandHandler>());

        var handlers = sp.GetServices<ICommandHandler>();

        Assert.Single(handlers);
        Assert.IsType<SomeCommandHandler>(handlers.First());
    }

    private record SomeNotification : INotification;
    private class SomeNotificationHandler : INotificationHandler<SomeNotification>
    {
        public Task HandleAsync(SomeNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public void AddNotificationHandler_RegistersAsINotificationHandler()
    {
        var sp = BuildServiceProvider(s => s.AddNotificationHandler<SomeNotificationHandler>());

        var handlers = sp.GetServices<INotificationHandler>();

        Assert.Single(handlers);
        Assert.IsType<SomeNotificationHandler>(handlers.First());
    }

    [Fact]
    public void AddMediator_WithConfigure_AppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(options =>
        {
            options.DefaultNotificationStrategy = NotificationStrategy.Parallel;
            options.DefaultCommandStrategy = CommandStrategy.Default;
        });
        var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var mediator = scope.ServiceProvider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }
}
