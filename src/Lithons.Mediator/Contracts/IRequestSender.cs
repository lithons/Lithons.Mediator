namespace Lithons.Mediator.Contracts;

public interface IRequestSender
{
    Task<T> SendAsync<T>(IRequest<T> request, CancellationToken cancellationToken = default);
}
