using BenchmarkDotNet.Attributes;
using Lithons.Mediator.Abstractions.Contracts;
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
        public Task<string> HandleAsync(EchoRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Value);
    }

    private record AddCommand(int A, int B) : ICommand<int>;

    private class AddCommandHandler : ICommandHandler<AddCommand, int>
    {
        public Task<int> HandleAsync(AddCommand command, CancellationToken cancellationToken)
            => Task.FromResult(command.A + command.B);
    }

    private record PingCommand : ICommand;

    private class PingCommandHandler : ICommandHandler<PingCommand>
    {
        public Task HandleAsync(PingCommand command, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private record OrderPlacedNotification(int OrderId) : INotification;

    private class OrderHandler1 : INotificationHandler<OrderPlacedNotification>
    {
        public Task HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private class OrderHandler2 : INotificationHandler<OrderPlacedNotification>
    {
        public Task HandleAsync(OrderPlacedNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private static readonly EchoRequest _echoRequest = new("hello");
    private static readonly AddCommand _addCommand = new(3, 4);
    private static readonly PingCommand _pingCommand = new();
    private static readonly OrderPlacedNotification _notification = new(42);

    private IServiceScope _scope = null!;
    private IMediator _mediator = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddMediator();
        services.AddRequestHandler<EchoRequestHandler>();
        services.AddCommandHandler<AddCommandHandler>();
        services.AddCommandHandler<PingCommandHandler>();
        services.AddNotificationHandler<OrderHandler1>();
        services.AddNotificationHandler<OrderHandler2>();

        var provider = services.BuildServiceProvider();
        _scope = provider.CreateScope();
        _mediator = _scope.ServiceProvider.GetRequiredService<IMediator>();
    }

    [GlobalCleanup]
    public void Cleanup() => _scope.Dispose();

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
}