namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Defines the ability to send request messages and receive typed responses.
/// </summary>
public interface IRequestSender
{
    /// <summary>
    /// Sends the specified request and returns the response asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the response.</typeparam>
    /// <param name="request">The request message to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation, with the handler's response as the result.</returns>
    Task<T> SendAsync<T>(IRequest<T> request, CancellationToken cancellationToken = default);
}
