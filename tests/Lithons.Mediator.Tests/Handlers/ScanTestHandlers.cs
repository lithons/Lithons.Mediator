using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Tests.ScanHandlers;

internal record ScanRequest(string Value) : IRequest<string>;

internal class ScanRequestHandler : IRequestHandler<ScanRequest, string>
{
    public Task<string> Handle(ScanRequest request, CancellationToken cancellationToken)
        => Task.FromResult(request.Value);
}

internal record ScanNotification : INotification;

internal class ScanNotificationHandler : INotificationHandler<ScanNotification>
{
    public Task Handle(ScanNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

internal record ScanCommand(int A, int B) : ICommand<int>;

internal class ScanCommandHandler : ICommandHandler<ScanCommand, int>
{
    public Task<int> Handle(ScanCommand command, CancellationToken cancellationToken)
        => Task.FromResult(command.A + command.B);
}
