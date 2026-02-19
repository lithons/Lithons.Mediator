using Lithons.Mediator.Abstractions.Middleware.Command;
using Lithons.Mediator.Abstractions.Middleware.Command.Contracts;

namespace Lithons.Mediator.Middleware.Command;

public class CommandPipeline : ICommandPipeline
{
    private readonly IServiceProvider _serviceProvider;
    private CommandMiddlewareDelegate? _pipeline;

    public CommandPipeline(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public CommandMiddlewareDelegate Pipeline => _pipeline ??= Setup(x => x.UseCommandHandlers());

    public CommandMiddlewareDelegate Setup(Action<ICommandPipelineBuilder>? setup = default)
    {
        var builder = new CommandPipelineBuilder(_serviceProvider);
        setup?.Invoke(builder);
        _pipeline = builder.Build();
        return _pipeline;
    }

    public async Task InvokeAsync(CommandContext context) => await Pipeline(context);
}
