using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;

namespace Lithons.Mediator.Middleware.Command;

public class CommandPipelineBuilder : ICommandPipelineBuilder
{
    private readonly List<Func<CommandMiddlewareDelegate, CommandMiddlewareDelegate>> _components = [];

    public CommandPipelineBuilder(IServiceProvider serviceProvider)
    {
        ApplicationServices = serviceProvider;
    }

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
    public IServiceProvider ApplicationServices { get; set; }

    public ICommandPipelineBuilder Use(Func<CommandMiddlewareDelegate, CommandMiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    public CommandMiddlewareDelegate Build()
    {
        CommandMiddlewareDelegate pipeline = _ => ValueTask.CompletedTask;
        for (var i = _components.Count - 1; i >= 0; i--)
            pipeline = _components[i](pipeline);
        return pipeline;
    }
}
