namespace Lithons.Mediator.Abstractions.Contracts;

public interface IRequestSender
{
    Task<T> SendAsync<T>(IRequest<T> request, CancellationToken cancellationToken = default);
}
