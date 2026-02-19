using Lithons.Mediator.Middleware.Request.Contracts;

namespace Lithons.Mediator.Middleware.Request;

public class RequestPipeline : IRequestPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private RequestMiddlewareDelegate? _pipeline;

    public RequestPipeline(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public RequestMiddlewareDelegate Pipeline => _pipeline ??= Setup(x => x.UseRequestHandlers());

    public RequestMiddlewareDelegate Setup(Action<IRequestPipelineBuilder>? setup = default)
    {
        var builder = new RequestPipelineBuilder(_serviceProvider);
        setup?.Invoke(builder);
        _pipeline = builder.Build();
        return _pipeline;
    }

    public async Task ExecuteAsync(RequestContext context) => await Pipeline(context);
}
