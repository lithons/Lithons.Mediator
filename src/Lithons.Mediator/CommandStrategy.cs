using Lithons.Mediator.CommandStrategies;

namespace Lithons.Mediator;

/// <summary>
/// Provides built-in <see cref="Abstractions.Contracts.ICommandStrategy"/> instances.
/// </summary>
public static class CommandStrategy
{
    /// <summary>
    /// Gets the default strategy that executes commands synchronously within the current scope.
    /// </summary>
    public static readonly DefaultStrategy Default = new();

    /// <summary>
    /// Gets a strategy that enqueues commands onto the background channel for deferred processing.
    /// </summary>
    public static readonly BackgroundStrategy Background = new();
}
