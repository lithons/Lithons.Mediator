namespace Lithons.Mediator.Abstractions.Middleware.Command;

/// <summary>
/// Represents a step in the command middleware pipeline.
/// </summary>
/// <param name="context">The <see cref="CommandContext"/> for the current command.</param>
public delegate ValueTask CommandMiddlewareDelegate(CommandContext context);
