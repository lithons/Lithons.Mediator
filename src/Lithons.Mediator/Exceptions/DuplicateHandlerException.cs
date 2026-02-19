namespace Lithons.Mediator.Exceptions;

public class DuplicateHandlerException(Type messageType)
    : MediatorException($"Multiple handlers registered for '{messageType.Name}'. Only one handler is allowed.")
{
    public Type MessageType { get; } = messageType;
}
