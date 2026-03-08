using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Example.Blazor.Services;

namespace Lithons.Mediator.Example.Blazor.ExceptionHandlers;

public class GlobalExceptionHandler(ExceptionLog log) : IExceptionHandler
{
    public ValueTask<bool> Handle(Exception exception, object message, CancellationToken cancellationToken)
    {
        log.Add($"[Global] {message.GetType().Name} failed: {exception.Message}");
        return ValueTask.FromResult(true);
    }
}
