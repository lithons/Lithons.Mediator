namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Marker interface for request messages.
/// </summary>
public interface IRequest;

/// <summary>
/// Represents a request message that produces a response of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the response returned by the handler.</typeparam>
public interface IRequest<T> : IRequest;
