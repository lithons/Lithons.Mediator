using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Abstractions.Middleware.Command;

public class CommandContext(
    ICommand command,
    ICommandStrategy commandStrategy,
    Type? resultType,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken)
{
    public ICommand Command { get; init; } = command;
    public ICommandStrategy CommandStrategy { get; init; } = commandStrategy;
    public Type? ResultType { get; init; } = resultType;
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
    public CancellationToken CancellationToken { get; init; } = cancellationToken;
    public object? Result { get; set; }

    private IDictionary<object, object>? _items;
    public IDictionary<object, object> Items => _items ??= new Dictionary<object, object>();
}
