namespace Lithons.Mediator.Abstractions.Middleware.Request;

/// <summary>
/// Represents a step in the request middleware pipeline.
/// </summary>
/// <param name="context">The <see cref="RequestContext"/> for the current request.</param>
public delegate ValueTask RequestMiddlewareDelegate(RequestContext context);
