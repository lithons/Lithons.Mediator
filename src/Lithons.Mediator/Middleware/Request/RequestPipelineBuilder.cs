using Lithons.Mediator.Middleware.Request.Contracts;

namespace Lithons.Mediator.Middleware.Request;

public class RequestPipelineBuilder : IRequestPipelineBuilder
{
    private readonly List<Func<RequestMiddlewareDelegate, RequestMiddlewareDelegate>> _components = [];

    public RequestPipelineBuilder(IServiceProvider serviceProvider)
    {
        ApplicationServices = serviceProvider;
    }

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
    public IServiceProvider ApplicationServices { get; set; }

    public IRequestPipelineBuilder Use(Func<RequestMiddlewareDelegate, RequestMiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public RequestMiddlewareDelegate Build()
    {
        RequestMiddlewareDelegate pipeline = _ => ValueTask.CompletedTask;
        for (var i = _components.Count - 1; i >= 0; i--)
            pipeline = _components[i](pipeline);
        return pipeline;
    }
}
