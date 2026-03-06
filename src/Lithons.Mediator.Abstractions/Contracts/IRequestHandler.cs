namespace Lithons.Mediator.Abstractions.Contracts;

public interface IRequestHandler;

public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
