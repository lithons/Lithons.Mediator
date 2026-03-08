namespace Lithons.Mediator.Exceptions;

/// <summary>
/// Base class for all exceptions thrown by the mediator.
/// </summary>
public abstract class MediatorException(string message, Exception? innerException = null)
    : Exception(message, innerException);
