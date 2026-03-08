namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Marker interface for request handler implementations.
/// </summary>
public interface IRequestHandler;

/// <summary>
/// Defines a handler for a request of type <typeparamref name="TRequest"/> that returns <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of the request to handle.</typeparam>
/// <typeparam name="TResponse">The type of the response produced by the handler.</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IRequestHandler
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the specified request and returns a response asynchronously.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the handler's response as the result.</returns>
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
