using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Tests.Handlers;

internal record ScanRequest(string Value) : IRequest<string>;

internal class ScanRequestHandler : IRequestHandler<ScanRequest, string>
{
    public Task<string> Handle(ScanRequest request, CancellationToken cancellationToken)
        => Task.FromResult(request.Value);
}

internal record ScanNotification : INotification;

internal class ScanNotificationHandler : INotificationHandler<ScanNotification>
{
    public Task HandleAsync(ScanNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

internal record ScanCommand(int A, int B) : ICommand<int>;

internal class ScanCommandHandler : ICommandHandler<ScanCommand, int>
{
    public Task<int> HandleAsync(ScanCommand command, CancellationToken cancellationToken)
        => Task.FromResult(command.A + command.B);
}
