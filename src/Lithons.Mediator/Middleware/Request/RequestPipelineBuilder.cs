using Lithons.Mediator.Abstractions.Middleware.Request;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;

namespace Lithons.Mediator.Middleware.Request;

/// <summary>
/// Default implementation of <see cref="IRequestPipelineBuilder"/>.
/// </summary>
public class RequestPipelineBuilder : IRequestPipelineBuilder
{
    private readonly List<Func<RequestMiddlewareDelegate, RequestMiddlewareDelegate>> _components = [];

    /// <summary>
    /// Initializes a new instance of <see cref="RequestPipelineBuilder"/>.
    /// </summary>
    /// <param name="serviceProvider">The application-level service provider.</param>
    public RequestPipelineBuilder(IServiceProvider serviceProvider)
    {
        ApplicationServices = serviceProvider;
    }

    /// <inheritdoc />
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    /// <inheritdoc />
    public IServiceProvider ApplicationServices { get; set; }

    /// <inheritdoc />
    public IRequestPipelineBuilder Use(Func<RequestMiddlewareDelegate, RequestMiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    /// <inheritdoc />
    public RequestMiddlewareDelegate Build()
    {
        RequestMiddlewareDelegate pipeline = _ => ValueTask.CompletedTask;
        for (var i = _components.Count - 1; i >= 0; i--)
            pipeline = _components[i](pipeline);
        return pipeline;
    }
}
