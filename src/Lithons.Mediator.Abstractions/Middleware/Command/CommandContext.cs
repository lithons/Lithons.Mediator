using Lithons.Mediator.Abstractions.Contracts;

namespace Lithons.Mediator.Abstractions.Middleware.Command;

/// <summary>
/// Carries the state of a command as it flows through the command middleware pipeline.
/// </summary>
public class CommandContext(
    ICommand command,
    ICommandStrategy commandStrategy,
    Type? resultType,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken)
{
    /// <summary>
    /// Gets the command message being processed.
    /// </summary>
    public ICommand Command { get; init; } = command;

    /// <summary>
    /// Gets the strategy used to execute the command.
    /// </summary>
    public ICommandStrategy CommandStrategy { get; init; } = commandStrategy;

    /// <summary>
    /// Gets the expected result type, or <see langword="null"/> for void commands.
    /// </summary>
    public Type? ResultType { get; init; } = resultType;

    /// <summary>
    /// Gets the scoped service provider for the current command.
    /// </summary>
    public IServiceProvider ServiceProvider { get; } = serviceProvider;

    /// <summary>
    /// Gets a token that can cancel the pipeline execution.
    /// </summary>
    public CancellationToken CancellationToken { get; init; } = cancellationToken;

    /// <summary>
    /// Gets or sets the result produced by the handler, or <see langword="null"/> for void commands.
    /// </summary>
    public object? Result { get; set; }

    private IDictionary<object, object>? _items;

    /// <summary>
    /// Gets a mutable property bag for passing arbitrary data through the pipeline.
    /// </summary>
    public IDictionary<object, object> Items => _items ??= new Dictionary<object, object>();
}
