using Lithons.Mediator.Abstractions.Middleware.Request;
using Lithons.Mediator.Abstractions.Middleware.Request.Contracts;

namespace Lithons.Mediator.Middleware.Request;

/// <summary>
/// Default implementation of <see cref="IRequestPipeline"/>.
/// </summary>
public class RequestPipeline : IRequestPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private RequestMiddlewareDelegate? _pipeline;

    /// <summary>
    /// Initializes a new instance of <see cref="RequestPipeline"/>.
    /// </summary>
    /// <param name="serviceProvider">The application-level service provider.</param>
    public RequestPipeline(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public RequestMiddlewareDelegate Pipeline => _pipeline ??= Setup(x => x.UseRequestHandlers());

    /// <inheritdoc />
    public RequestMiddlewareDelegate Setup(Action<IRequestPipelineBuilder>? setup = default)
    {
        var builder = new RequestPipelineBuilder(_serviceProvider);
        setup?.Invoke(builder);
        _pipeline = builder.Build();
        return _pipeline;
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(RequestContext context) => await Pipeline(context);
}
