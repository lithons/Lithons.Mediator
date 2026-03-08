using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Abstractions.Middleware.Request;

/// <summary>
/// Carries the state of a request as it flows through the request middleware pipeline.
/// </summary>
public class RequestContext(IRequest request, Type responseType, IServiceProvider serviceProvider, CancellationToken cancellationToken)
{
    /// <summary>
    /// Gets the request message being processed.
    /// </summary>
    public IRequest Request { get; init; } = request;

    /// <summary>
    /// Gets the expected response type for the request.
    /// </summary>
    public Type ResponseType { get; init; } = responseType;

    /// <summary>
    /// Gets the scoped service provider for the current request.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets a token that can cancel the pipeline execution.
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = cancellationToken;

    /// <summary>
    /// Gets or sets the response produced by the handler.
    /// </summary>
    public object Response { get; set; } = null!;
}
