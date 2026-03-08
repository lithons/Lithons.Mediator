namespace Lithons.Mediator.Abstractions.Middleware.Request.Contracts;

/// <summary>
/// Defines a middleware component in the request pipeline.
/// </summary>
public interface IRequestMiddleware
{
    /// <summary>
    /// Processes the current <paramref name="context"/> and optionally invokes the next middleware in the pipeline.
    /// </summary>
    /// <param name="context">The context for the current request.</param>
    ValueTask InvokeAsync(RequestContext context);
}
