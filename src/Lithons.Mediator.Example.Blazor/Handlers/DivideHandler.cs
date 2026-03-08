using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Example.Blazor.Handlers;

public record DivideRequest(double Numerator, double Denominator) : IRequest<double>;

public class DivideHandler : IRequestHandler<DivideRequest, double>
{
    public Task<double> Handle(DivideRequest request, CancellationToken cancellationToken)
    {
        if (request.Denominator == 0)
            throw new DivideByZeroException("Cannot divide by zero.");

        return Task.FromResult(request.Numerator / request.Denominator);
    }
}
