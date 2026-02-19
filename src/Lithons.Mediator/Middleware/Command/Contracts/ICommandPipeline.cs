namespace Lithons.Mediator.Middleware.Command.Contracts;

public interface ICommandPipeline
{
    CommandMiddlewareDelegate Setup(Action<ICommandPipelineBuilder> setup);
    CommandMiddlewareDelegate Pipeline { get; }
    Task InvokeAsync(CommandContext context);
}
