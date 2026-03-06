namespace Lithons.Mediator.Example.Blazor.Handlers;

public record StringReverseRequest(string Value) : IRequest<string>;
public class StringReverseHandler : IRequestHandler<StringReverseRequest, string>
{
    private readonly IStringService _stringService;

    public StringReverseHandler(IStringService stringService)
    {
        _stringService = stringService;
    }
    public Task<string> Handle(StringReverseRequest request, CancellationToken cancellationToken)
    {
        var reversed = _stringService.Reverse(request.Value);
        return Task.FromResult(reversed);
    }
}

public interface IStringService
{
    string Reverse(string value);
}

public class StringService : IStringService
{
    public string Reverse(string value)
    {
        var reversed = new string([.. value.Reverse()]);
        return reversed;
    }
}