using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Samples.Blazor.Handlers;

public record StringReverseRequest(string Value) : IRequest<string>;
public class StringReverseHandler : IRequestHandler<StringReverseRequest, string>
{
    public Task<string> HandleAsync(StringReverseRequest request, CancellationToken cancellationToken)
    {
        var reversed = new string(request.Value.Reverse().ToArray());
        return Task.FromResult(reversed);
    }
}