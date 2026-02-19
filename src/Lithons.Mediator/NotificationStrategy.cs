using Lithons.Mediator.NotificationStrategies;

namespace Lithons.Mediator;

public static class NotificationStrategy
{
    public static readonly SequentialStrategy Sequential = new();
    public static readonly ParallelStrategy Parallel = new();
}
