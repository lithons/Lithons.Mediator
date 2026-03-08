using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Abstractions.Middleware.Command;

namespace Lithons.Mediator.Abstractions.Contexts;

/// <summary>
/// Provides contextual data passed to an <see cref="ICommandStrategy"/> when executing a command.
/// </summary>
/// <param name="CommandContext">The command pipeline context carrying the command and its execution state.</param>
/// <param name="Handler">The resolved handler that will process the command.</param>
/// <param name="ServiceProvider">The scoped service provider for the current execution.</param>
/// <param name="CancellationToken">A token to cancel the operation.</param>
public record CommandStrategyContext(
    CommandContext CommandContext,
    ICommandHandler Handler,
    IServiceProvider ServiceProvider,
    CancellationToken CancellationToken = default);
