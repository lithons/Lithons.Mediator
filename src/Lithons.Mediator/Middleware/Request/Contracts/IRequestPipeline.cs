namespace Lithons.Mediator.Middleware.Request.Contracts;

public interface IRequestPipeline
{
    RequestMiddlewareDelegate Setup(Action<IRequestPipelineBuilder> setup);
    RequestMiddlewareDelegate Pipeline { get; }
    Task ExecuteAsync(RequestContext context);
}
