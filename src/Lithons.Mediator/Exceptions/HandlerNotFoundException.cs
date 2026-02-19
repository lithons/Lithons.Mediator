namespace Lithons.Mediator.Exceptions;

public class HandlerNotFoundException : MediatorException
{
    public Type MessageType { get; }

    public HandlerNotFoundException(Type messageType, Exception? innerException = null)
        : base($"No handler registered for '{messageType.Name}'.", innerException)
    {
        MessageType = messageType;
    }
}
