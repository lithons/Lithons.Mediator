namespace Lithons.Mediator.Abstractions.Middleware.Command;

public delegate ValueTask CommandMiddlewareDelegate(CommandContext context);
