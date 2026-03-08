using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Extensions;
using Lithons.Mediator.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lithons.Mediator.Tests.Services;

public class BackgroundCommandProcessingIntegrationTests
{
    private record IncrementCommand(int Value) : ICommand<int>;

    private class IncrementCommandHandler : ICommandHandler<IncrementCommand, int>
    {
        public int? LastResult { get; private set; }

        public Task<int> Handle(IncrementCommand command, CancellationToken cancellationToken)
        {
            LastResult = command.Value + 1;
            return Task.FromResult(LastResult.Value);
        }
    }

    private record FireAndForgetCommand(string Tag) : ICommand;

    private class FireAndForgetHandler : ICommandHandler<FireAndForgetCommand>
    {
        private readonly TaskCompletionSource _processed = new();
        public Task Processed => _processed.Task;
        public string? ReceivedTag { get; private set; }

        public Task Handle(FireAndForgetCommand command, CancellationToken cancellationToken)
        {
            ReceivedTag = command.Tag;
            _processed.TrySetResult();
            return Task.CompletedTask;
        }
    }

    [Fact]
    public void AddBackgroundCommandProcessing_Registers_ICommandsChannel()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(cfg => cfg.AddBackgroundCommandProcessing());

        using var sp = services.BuildServiceProvider();

        var channel = sp.GetService<ICommandsChannel>();
        Assert.NotNull(channel);
        Assert.IsType<DefaultCommandsChannel>(channel);
    }

    [Fact]
    public void AddBackgroundCommandProcessing_Registers_HostedService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(cfg => cfg.AddBackgroundCommandProcessing());

        using var sp = services.BuildServiceProvider();

        var hostedServices = sp.GetServices<IHostedService>();
        Assert.Contains(hostedServices, s => s is CommandsBackgroundService);
    }

    [Fact]
    public async Task EndToEnd_BackgroundCommand_IsProcessedByBackgroundService()
    {
        var handler = new FireAndForgetHandler();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(cfg =>
        {
            cfg.AddCommandHandler<FireAndForgetHandler>();
            cfg.AddBackgroundCommandProcessing();
        });
        // Override the handler registration with our tracked instance
        services.AddSingleton<ICommandHandler<FireAndForgetCommand>>(handler);

        using var sp = services.BuildServiceProvider();

        // Start the background service
        var hostedService = sp.GetServices<IHostedService>()
            .OfType<CommandsBackgroundService>()
            .Single();
        await hostedService.StartAsync(TestContext.Current.CancellationToken);

        // Send a command via mediator with Background strategy
        var mediator = sp.GetRequiredService<IMediator>();
        await mediator.SendAsync(new FireAndForgetCommand("integration-test"), CommandStrategy.Background, TestContext.Current.CancellationToken);

        // Wait for background processing (with timeout)
        await handler.Processed.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        Assert.Equal("integration-test", handler.ReceivedTag);

        await hostedService.StopAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task EndToEnd_ResultCommand_BackgroundStrategy_ReturnsDefault_HandlerStillExecutes()
    {
        var handler = new IncrementCommandHandler();
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator(cfg =>
        {
            cfg.AddCommandHandler<IncrementCommandHandler>();
            cfg.AddBackgroundCommandProcessing();
        });
        services.AddSingleton<ICommandHandler<IncrementCommand, int>>(handler);

        using var sp = services.BuildServiceProvider();

        var hostedService = sp.GetServices<IHostedService>()
            .OfType<CommandsBackgroundService>()
            .Single();
        await hostedService.StartAsync(TestContext.Current.CancellationToken);

        var mediator = sp.GetRequiredService<IMediator>();
        var result = await mediator.SendAsync(new IncrementCommand(10), CommandStrategy.Background, TestContext.Current.CancellationToken);

        // Caller gets default immediately
        Assert.Equal(default, result);

        // Wait for background handler to complete
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (handler.LastResult is null && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10, TestContext.Current.CancellationToken);
        }

        // Handler was still invoked in the background
        Assert.Equal(11, handler.LastResult);

        await hostedService.StopAsync(TestContext.Current.CancellationToken);
    }
}
