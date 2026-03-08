namespace Lithons.Mediator.Abstractions.Middleware.Request.Contracts;

/// <summary>
/// Represents the request middleware pipeline used to process request messages.
/// </summary>
public interface IRequestPipeline
{
    /// <summary>
    /// Configures the pipeline using the provided builder action and returns the composed delegate.
    /// </summary>
    /// <param name="setup">An action that adds middleware components to the pipeline builder.</param>
    /// <returns>The composed <see cref="RequestMiddlewareDelegate"/>.</returns>
    RequestMiddlewareDelegate Setup(Action<IRequestPipelineBuilder> setup);

    /// <summary>
    /// Gets the composed middleware delegate, building the default pipeline on first access.
    /// </summary>
    RequestMiddlewareDelegate Pipeline { get; }

    /// <summary>
    /// Executes the pipeline for the given context.
    /// </summary>
    /// <param name="context">The context carrying the request and its execution state.</param>
    Task ExecuteAsync(RequestContext context);
}
