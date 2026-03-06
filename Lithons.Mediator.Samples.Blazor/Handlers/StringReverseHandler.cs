using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Samples.Blazor.Handlers;

public record StringReverseRequest(string Value) : IRequest<string>;
public class StringReverseHandler : IRequestHandler<StringReverseRequest, string>
{
    public Task<string> Handle(StringReverseRequest request, CancellationToken cancellationToken)
    {
        var reversed = new string([.. request.Value.Reverse()]);
        return Task.FromResult(reversed);
    }
}