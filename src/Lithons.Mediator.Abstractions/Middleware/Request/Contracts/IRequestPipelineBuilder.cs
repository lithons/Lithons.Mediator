namespace Lithons.Mediator.Abstractions.Middleware.Request.Contracts;

/// <summary>
/// Provides a mechanism to configure the request middleware pipeline.
/// </summary>
public interface IRequestPipelineBuilder
{
    /// <summary>
    /// Gets a collection of properties shared across the pipeline-building process.
    /// </summary>
    IDictionary<string, object?> Properties { get; }

    /// <summary>
    /// Gets the application-level service provider.
    /// </summary>
    IServiceProvider ApplicationServices { get; }

    /// <summary>
    /// Adds a middleware factory to the pipeline.
    /// </summary>
    /// <param name="middleware">A function that wraps the next delegate with the new middleware component.</param>
    /// <returns>The current <see cref="IRequestPipelineBuilder"/> for chaining.</returns>
    IRequestPipelineBuilder Use(Func<RequestMiddlewareDelegate, RequestMiddlewareDelegate> middleware);

    /// <summary>
    /// Builds the composed pipeline delegate from all registered middleware components.
    /// </summary>
    /// <returns>The composed <see cref="RequestMiddlewareDelegate"/>.</returns>
    RequestMiddlewareDelegate Build();
}
