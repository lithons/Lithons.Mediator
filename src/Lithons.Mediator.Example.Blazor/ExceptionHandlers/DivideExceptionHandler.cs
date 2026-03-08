using Lithons.Mediator.Abstractions.Contracts;
using Lithons.Mediator.Example.Blazor.Handlers;
using Lithons.Mediator.Example.Blazor.Services;

namespace Lithons.Mediator.Example.Blazor.ExceptionHandlers;

public class DivideExceptionHandler(ExceptionLog log) : IExceptionHandler<DivideRequest>
{
    public ValueTask<bool> Handle(Exception exception, DivideRequest message, CancellationToken cancellationToken)
    {
        log.Add($"[Typed] DivideRequest({message.Numerator} / {message.Denominator}) failed: {exception.Message}");
        return ValueTask.FromResult(true);
    }
}
