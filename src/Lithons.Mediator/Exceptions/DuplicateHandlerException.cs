namespace Lithons.Mediator.Exceptions;

/// <summary>
/// Thrown when more than one handler is registered for a message type that expects exactly one handler.
/// </summary>
public class DuplicateHandlerException(Type messageType)
    : MediatorException($"Multiple handlers registered for '{messageType.Name}'. Only one handler is allowed.")
{
    /// <summary>
    /// Gets the message type for which duplicate handlers were found.
    /// </summary>
    public Type MessageType { get; } = messageType;
}
