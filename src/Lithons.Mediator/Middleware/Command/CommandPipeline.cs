using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;

namespace Lithons.Mediator.Middleware.Command;

/// <summary>
/// Default implementation of <see cref="ICommandPipeline"/>.
/// </summary>
public class CommandPipeline : ICommandPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private CommandMiddlewareDelegate? _pipeline;

    /// <summary>
    /// Initializes a new instance of <see cref="CommandPipeline"/>.
    /// </summary>
    /// <param name="serviceProvider">The application-level service provider.</param>
    public CommandPipeline(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    /// <inheritdoc />
    public CommandMiddlewareDelegate Pipeline => _pipeline ??= Setup(x => x.UseCommandHandlers());

    /// <inheritdoc />
    public CommandMiddlewareDelegate Setup(Action<ICommandPipelineBuilder>? setup = default)
    {
        var builder = new CommandPipelineBuilder(_serviceProvider);
        setup?.Invoke(builder);
        _pipeline = builder.Build();
        return _pipeline;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(CommandContext context) => await Pipeline(context);
}
