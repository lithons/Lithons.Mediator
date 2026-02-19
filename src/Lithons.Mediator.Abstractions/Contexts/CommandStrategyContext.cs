using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;

namespace Lithons.Mediator.Abstractions.Contexts;

public record CommandStrategyContext(
    CommandContext CommandContext,
    ICommandHandler Handler,
    IServiceProvider ServiceProvider,
    CancellationToken CancellationToken = default);
