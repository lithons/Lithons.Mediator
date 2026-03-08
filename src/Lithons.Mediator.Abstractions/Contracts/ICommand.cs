namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Marker interface for command messages.
/// </summary>
public interface ICommand;

/// <summary>
/// Represents a command message that produces a result of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the result returned by the handler.</typeparam>
public interface ICommand<T> : ICommand;
