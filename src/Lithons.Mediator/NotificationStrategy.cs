using Lithons.Mediator.NotificationStrategies;

namespace Lithons.Mediator;

/// <summary>
/// Provides built-in <see cref="Abstractions.Contracts.INotificationStrategy"/> instances.
/// </summary>
public static class NotificationStrategy
{
    /// <summary>
    /// Gets a strategy that invokes handlers one at a time, in registration order.
    /// </summary>
    public static readonly SequentialStrategy Sequential = new();

    /// <summary>
    /// Gets a strategy that invokes all handlers concurrently.
    /// </summary>
    public static readonly ParallelStrategy Parallel = new();
}
