using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Example.Blazor.Services;

namespace Lithons.Mediator.Example.Blazor.Handlers;

public record AuditLogCommand(string Action, string Detail) : ICommand;

public class AuditLogCommandHandler(AuditLog auditLog) : ICommandHandler<AuditLogCommand>
{
    public async Task Handle(AuditLogCommand command, CancellationToken cancellationToken)
    {
        // Simulate async work (e.g. writing to a database or external service)
        await Task.Delay(500, cancellationToken);
        auditLog.Add(command.Action, command.Detail);
    }
}
