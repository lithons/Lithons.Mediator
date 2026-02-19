namespace Lithons.Mediator.Exceptions;

public abstract class MediatorException(string message, Exception? innerException = null)
    : Exception(message, innerException);
