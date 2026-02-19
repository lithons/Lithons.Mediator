using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Notification.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;
using Lithons.Mediator.CommandStrategies;
using Lithons.Mediator.Exceptions;
using Lithons.Mediator.NotificationStrategies;
using Lithons.Mediator.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

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

    [Fact]
    public void AddMediator_ICommandSender_IsSameInstanceAsIMediator()
    {
        var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var sender = scope.ServiceProvider.GetRequiredService<ICommandSender>();

        Assert.Same(mediator, sender);
    }

    [Fact]
    public void AddMediator_INotificationSender_IsSameInstanceAsIMediator()
    {
        var sp = BuildServiceProvider();
        using var scope = sp.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

        Assert.Same(mediator, sender);
    }

    [Fact]
    public void AddMediator_CalledTwice_DoesNotRegisterDuplicateServices()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator();
        services.AddMediator();

        using var scope = services.BuildServiceProvider().CreateScope();
        var mediators = scope.ServiceProvider.GetServices<IMediator>().ToList();

        Assert.Single(mediators);
    }

    private class DuplicateSomeCommandHandler : ICommandHandler<SomeCommand, int>
    {
        public Task<int> HandleAsync(SomeCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.Value);
    }

    [Fact]
    public void AddCommandHandler_DuplicateHandlers_ThrowsDuplicateHandlerException()
    {
        Assert.Throws<DuplicateHandlerException>(() =>
        {
            BuildServiceProvider(s =>
            {
                s.AddCommandHandler<SomeCommandHandler>();
                s.AddCommandHandler<DuplicateSomeCommandHandler>();
            });
        });
    }

    [Fact]
    public void MediatorOptions_DefaultNotificationStrategy_IsSequential()
    {
        var options = new MediatorOptions();

        Assert.IsType<SequentialStrategy>(options.DefaultNotificationStrategy);
    }

    [Fact]
    public void MediatorOptions_DefaultCommandStrategy_IsDefault()
    {
        var options = new MediatorOptions();

        Assert.IsType<DefaultStrategy>(options.DefaultCommandStrategy);
    }
}
