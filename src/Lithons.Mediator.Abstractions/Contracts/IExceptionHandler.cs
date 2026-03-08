namespace Lithons.Mediator.Abstractions.Contracts;

/// <summary>
/// Global exception handler invoked for any unhandled exception in the mediator pipeline.
/// Return <c>true</c> to indicate the exception was handled and should not be rethrown.
/// </summary>
public interface IExceptionHandler
{
    ValueTask<bool> Handle(Exception exception, object message, CancellationToken cancellationToken);
}

/// <summary>
/// Typed exception handler invoked for unhandled exceptions raised while processing <typeparamref name="TMessage"/>.
/// Return <c>true</c> to indicate the exception was handled and should not be rethrown.
/// Typed handlers are tried before the global <see cref="IExceptionHandler"/>.
/// </summary>
public interface IExceptionHandler<in TMessage>
{
    ValueTask<bool> Handle(Exception exception, TMessage message, CancellationToken cancellationToken);
}
