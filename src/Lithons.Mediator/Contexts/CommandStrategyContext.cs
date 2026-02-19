using Lithons.Mediator.Contracts;
using Lithons.Mediator.Middleware.Command;

namespace Lithons.Mediator.Contexts;

public record CommandStrategyContext(
    CommandContext CommandContext,
    ICommandHandler Handler,
    IServiceProvider ServiceProvider,
    CancellationToken CancellationToken = default);
