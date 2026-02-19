using Lithons.Mediator.CommandStrategies;

namespace Lithons.Mediator;

public static class CommandStrategy
{
    public static readonly DefaultStrategy Default = new();
    public static readonly BackgroundStrategy Background = new();
}
