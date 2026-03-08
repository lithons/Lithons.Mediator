namespace Lithons.Mediator.Exceptions;

/// <summary>
/// Thrown when no handler is registered for the requested message type.
/// </summary>
public class HandlerNotFoundException : MediatorException
{
    /// <summary>
    /// Gets the message type for which no handler was found.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="HandlerNotFoundException"/> for the specified message type.
    /// </summary>
    /// <param name="messageType">The type of the message for which no handler was found.</param>
    /// <param name="innerException">An optional inner exception.</param>
    public HandlerNotFoundException(Type messageType, Exception? innerException = null)
        : base($"No handler registered for '{messageType.Name}'.", innerException)
    {
        MessageType = messageType;
    }
}
