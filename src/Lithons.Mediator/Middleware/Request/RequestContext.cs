using Lithons.Mediator.Contracts;

namespace Lithons.Mediator.Middleware.Request;

public class RequestContext(IRequest request, Type responseType, IServiceProvider serviceProvider, CancellationToken cancellationToken)
{
    public IRequest Request { get; init; } = request;
    public Type ResponseType { get; init; } = responseType;
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public CancellationToken CancellationToken { get; init; } = cancellationToken;
    public object Response { get; set; } = null!;
}
