using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;

namespace Lithons.Mediator.Middleware.Command;

/// <summary>
/// Default implementation of <see cref="ICommandPipelineBuilder"/>.
/// </summary>
public class CommandPipelineBuilder : ICommandPipelineBuilder
{
    private readonly List<Func<CommandMiddlewareDelegate, CommandMiddlewareDelegate>> _components = [];

    /// <summary>
    /// Initializes a new instance of <see cref="CommandPipelineBuilder"/>.
    /// </summary>
    /// <param name="serviceProvider">The application-level service provider.</param>
    public CommandPipelineBuilder(IServiceProvider serviceProvider)
    {
        ApplicationServices = serviceProvider;
    }

    /// <inheritdoc />
    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    /// <inheritdoc />
    public IServiceProvider ApplicationServices { get; set; }

    /// <inheritdoc />
    public ICommandPipelineBuilder Use(Func<CommandMiddlewareDelegate, CommandMiddlewareDelegate> middleware)
    {
        _components.Add(middleware);
        return this;
    }

    /// <inheritdoc />
    public CommandMiddlewareDelegate Build()
    {
        CommandMiddlewareDelegate pipeline = _ => ValueTask.CompletedTask;
        for (var i = _components.Count - 1; i >= 0; i--)
            pipeline = _components[i](pipeline);
        return pipeline;
    }
}
