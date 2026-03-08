namespace Lithons.Mediator.Abstractions.Middleware.Command.Contracts;

/// <summary>
/// Defines a middleware component in the command pipeline.
/// </summary>
public interface ICommandMiddleware
{
    /// <summary>
    /// Processes the current <paramref name="context"/> and optionally invokes the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The context for the current command.</param>
    ValueTask InvokeAsync(CommandContext context);
}
