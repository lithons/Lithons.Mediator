namespace Lithons.Mediator.Abstractions.Middleware.Request.Contracts;

public interface IRequestPipelineBuilder
{
    IDictionary<string, object?> Properties { get; }
    IServiceProvider ApplicationServices { get; }
    IRequestPipelineBuilder Use(Func<RequestMiddlewareDelegate, RequestMiddlewareDelegate> middleware);
    RequestMiddlewareDelegate Build();
}
